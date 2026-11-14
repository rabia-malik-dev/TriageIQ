using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Llm;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
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

        var llmCall = new LlmCall
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            CorrelationId = correlationId,
            PromptVersionId = promptVersion.Id,
            Provider = llmResult.Provider,
            Model = llmResult.Model,
            InputTokens = llmResult.InputTokens,
            OutputTokens = llmResult.OutputTokens,
            LatencyMs = llmResult.LatencyMs,
            EstimatedCostUsd = llmResult.EstimatedCostUsd,
            Succeeded = llmResult.Succeeded,
            ErrorMessage = llmResult.ErrorMessage,
            CreatedAt = clock.UtcNow,
        };

        if (!llmResult.Succeeded)
        {
            ticket.Status = TicketStatus.Failed;
            await triagePersistence.SaveTriageResultAsync(
                ticket,
                llmCall,
                BuildFailedAnalysisPlaceholder(ticket, promptVersion, llmCall, llmResult),
                cancellationToken);
            return;
        }

        TriageLlmResponse parsed;
        try
        {
            parsed = TriageResponseParser.Parse(llmResult.Content);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse LLM JSON for ticket {TicketId}", ticketId);
            llmCall.Succeeded = false;
            llmCall.ErrorMessage = ex.Message;
            ticket.Status = TicketStatus.Failed;
            await triagePersistence.SaveTriageResultAsync(
                ticket,
                llmCall,
                BuildFailedAnalysisPlaceholder(ticket, promptVersion, llmCall, llmResult),
                cancellationToken);
            return;
        }

        var threshold = options.Value.ConfidenceThreshold;
        var routing = parsed.Confidence >= threshold
            ? TriageRoutingDecision.AutoApproved
            : TriageRoutingDecision.NeedsReview;

        ticket.Status = routing == TriageRoutingDecision.AutoApproved
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
            RoutingDecision = routing,
            RawResponseJson = llmResult.Content,
            CreatedAt = clock.UtcNow,
        };

        await triagePersistence.SaveTriageResultAsync(ticket, llmCall, analysis, cancellationToken);

        logger.LogInformation(
            "Ticket {TicketId} triaged → {Status} (confidence {Confidence:P0})",
            ticket.Id,
            ticket.Status,
            parsed.Confidence);
    }

    private static TicketAnalysis BuildFailedAnalysisPlaceholder(
        Ticket ticket,
        PromptVersion promptVersion,
        LlmCall llmCall,
        LlmCompletionResult llmResult) =>
        new()
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
            CreatedAt = DateTimeOffset.UtcNow,
        };
}

public sealed class TriageOptions
{
    public const string SectionName = "Triage";

    public decimal ConfidenceThreshold { get; init; } = 0.85m;
}