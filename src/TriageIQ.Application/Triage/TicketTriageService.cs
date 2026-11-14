using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Llm;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
using TriageIQ.Application.Guardrails;
using TriageIQ.Domain.Entities;
using TriageIQ.Domain.Enums;

namespace TriageIQ.Application.Triage;

public sealed class TicketTriageService(
    ITicketRepository tickets,
    IPromptVersionRepository prompts,
    ITriagePersistence triagePersistence,
    ILlmClient llmClient,
    IClock clock,
    IOptions<TriageOptions> options,
    ILogger<TicketTriageService> logger)
{
    public async Task RunAsync(Guid ticketId, Guid correlationId, CancellationToken cancellationToken)
    {
        if (await triagePersistence.HasAnalysisAsync(ticketId, cancellationToken))
        {
            logger.LogInformation("Ticket {TicketId} already has analysis; skipping triage", ticketId);
            return;
        }

        var ticket = await tickets.FindByIdForUpdateAsync(ticketId, cancellationToken)
            ?? throw new NotFoundException($"Ticket {ticketId} was not found.");

        if (ticket.Status != TicketStatus.QueuedForTriage)
        {
            logger.LogInformation(
                "Ticket {TicketId} is in status {Status}; triage not applicable",
                ticketId,
                ticket.Status);
            return;
        }

        var promptVersion = await prompts.GetActiveAsync(cancellationToken)
            ?? throw new InvalidOperationException("No active prompt version configured.");

        ticket.BodyRedacted = PiiRedactor.Redact(ticket.Body);

        var preGuard = PreTriageGuardrails.Evaluate(ticket.Subject, ticket.Body);
        if (!preGuard.AllowLlmCall)
        {
            await SaveGuardrailBlockedAsync(
                ticket,
                promptVersion,
                correlationId,
                preGuard,
                cancellationToken);
            return;
        }

        var userPrompt = TriagePromptBuilder.BuildUserPrompt(
            promptVersion,
            ticket.Subject,
            ticket.BodyRedacted,
            ticket.Source);

        var llmRequest = new LlmCompletionRequest(
            TriagePromptBuilder.SystemPrompt,
            userPrompt,
            correlationId);

        var llmResult = await llmClient.CompleteAsync(llmRequest, cancellationToken);

        var llmCall = CreateLlmCall(ticket.Id, correlationId, promptVersion.Id, llmResult, clock.UtcNow);

        if (!llmResult.Succeeded)
        {
            await SaveFailedAsync(
                ticket,
                promptVersion,
                llmCall,
                llmResult,
                [GuardrailCodes.LlmFailed],
                cancellationToken);
            return;
        }

        var (resolved, parsed, finalContent) = await ResolveResponseAsync(
            llmResult.Content,
            userPrompt,
            correlationId,
            cancellationToken);

        if (!resolved)
        {
            llmCall.Succeeded = false;
            llmCall.ErrorMessage = "JSON parse and repair failed.";
            await SaveFailedAsync(
                ticket,
                promptVersion,
                llmCall,
                llmResult with { Content = finalContent ?? llmResult.Content },
                [GuardrailCodes.InvalidSchema],
                cancellationToken);
            return;
        }

        var triageOptions = options.Value;
        var postGuard = PostTriageGuardrails.Evaluate(
            parsed,
            triageOptions.ConfidenceThreshold,
            triageOptions.MaxEstimatedCostUsd,
            llmResult.EstimatedCostUsd);

        ticket.Status = postGuard.Routing == TriageRoutingDecision.AutoApproved
            ? TicketStatus.AutoApproved
            : TicketStatus.NeedsReview;

        var analysis = new TicketAnalysis
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PromptVersionId = promptVersion.Id,
            LlmCallId = llmCall.Id,
            Category = parsed.Category,
            Priority = parsed.Priority,
            Summary = parsed.Summary,
            Confidence = parsed.Confidence,
            RoutingDecision = postGuard.Routing,
            RawResponseJson = finalContent!,
            ReviewReason = postGuard.Violations.Count > 0
                ? string.Join(',', postGuard.Violations)
                : null,
            CreatedAt = clock.UtcNow,
        };

        await triagePersistence.SaveTriageResultAsync(ticket, llmCall, analysis, cancellationToken);

        logger.LogInformation(
            "Ticket {TicketId} triaged → {Status} (confidence {Confidence:P0}, guardrails {Guardrails})",
            ticket.Id,
            ticket.Status,
            parsed.Confidence,
            analysis.ReviewReason ?? "none");
    }

    private async Task<(bool Success, TriageLlmResponse Parsed, string Content)> ResolveResponseAsync(
        string content,
        string userPrompt,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        if (TryParseContent(content, out var parsed))
        {
            return (true, parsed, content);
        }

        var extracted = TriageJsonRepair.TryExtractJson(content);
        if (extracted is not null && TryParseContent(extracted, out parsed))
        {
            return (true, parsed, extracted);
        }

        var repair = await llmClient.CompleteAsync(
            new LlmCompletionRequest(
                TriagePromptBuilder.RepairSystemPrompt,
                $"Broken output:\n{content}\n\nOriginal ticket:\n{userPrompt}",
                correlationId),
            cancellationToken);

        if (!repair.Succeeded)
        {
            return (false, default!, content);
        }

        if (TryParseContent(repair.Content, out parsed))
        {
            return (true, parsed, repair.Content);
        }

        var repairedExtract = TriageJsonRepair.TryExtractJson(repair.Content);
        if (repairedExtract is not null && TryParseContent(repairedExtract, out parsed))
        {
            return (true, parsed, repairedExtract);
        }

        return (false, default!, repair.Content);
    }

    private static bool TryParseContent(string content, out TriageLlmResponse parsed)
    {
        try
        {
            parsed = TriageResponseParser.Parse(content);
            TriageOutputValidator.Validate(parsed);
            return true;
        }
        catch
        {
            parsed = default!;
            return false;
        }
    }

    private async Task SaveGuardrailBlockedAsync(
        Ticket ticket,
        PromptVersion promptVersion,
        Guid correlationId,
        GuardrailEvaluation preGuard,
        CancellationToken cancellationToken)
    {
        ticket.Status = TicketStatus.NeedsReview;

        var llmCall = new LlmCall
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            CorrelationId = correlationId,
            PromptVersionId = promptVersion.Id,
            Provider = "guardrail",
            Model = "pre-llm",
            InputTokens = 0,
            OutputTokens = 0,
            LatencyMs = 0,
            EstimatedCostUsd = 0,
            Succeeded = false,
            ErrorMessage = string.Join(',', preGuard.Violations),
            CreatedAt = clock.UtcNow,
        };

        var analysis = new TicketAnalysis
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PromptVersionId = promptVersion.Id,
            LlmCallId = llmCall.Id,
            Category = "security",
            Priority = "high",
            Summary = "Ticket blocked by pre-triage guardrails.",
            Confidence = 0,
            RoutingDecision = TriageRoutingDecision.NeedsReview,
            RawResponseJson = "{}",
            ReviewReason = string.Join(',', preGuard.Violations),
            CreatedAt = clock.UtcNow,
        };

        await triagePersistence.SaveTriageResultAsync(ticket, llmCall, analysis, cancellationToken);

        logger.LogWarning(
            "Ticket {TicketId} blocked by guardrails: {Reasons}",
            ticket.Id,
            analysis.ReviewReason);
    }

    private async Task SaveFailedAsync(
        Ticket ticket,
        PromptVersion promptVersion,
        LlmCall llmCall,
        LlmCompletionResult llmResult,
        IReadOnlyList<string> reasons,
        CancellationToken cancellationToken)
    {
        ticket.Status = TicketStatus.Failed;

        var analysis = new TicketAnalysis
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PromptVersionId = promptVersion.Id,
            LlmCallId = llmCall.Id,
            Category = "unknown",
            Priority = "low",
            Summary = "Triage failed",
            Confidence = 0,
            RoutingDecision = TriageRoutingDecision.NeedsReview,
            RawResponseJson = llmResult.Content,
            ReviewReason = string.Join(',', reasons),
            CreatedAt = clock.UtcNow,
        };

        await triagePersistence.SaveTriageResultAsync(ticket, llmCall, analysis, cancellationToken);
    }

    private static LlmCall CreateLlmCall(
        Guid ticketId,
        Guid correlationId,
        Guid promptVersionId,
        LlmCompletionResult llmResult,
        DateTimeOffset createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            CorrelationId = correlationId,
            PromptVersionId = promptVersionId,
            Provider = llmResult.Provider,
            Model = llmResult.Model,
            InputTokens = llmResult.InputTokens,
            OutputTokens = llmResult.OutputTokens,
            LatencyMs = llmResult.LatencyMs,
            EstimatedCostUsd = llmResult.EstimatedCostUsd,
            Succeeded = llmResult.Succeeded,
            ErrorMessage = llmResult.ErrorMessage,
            CreatedAt = createdAt,
        };
}

public sealed class TriageOptions
{
    public const string SectionName = "Triage";

    public decimal ConfidenceThreshold { get; init; } = 0.85m;

    public decimal MaxEstimatedCostUsd { get; init; } = 0.25m;
}