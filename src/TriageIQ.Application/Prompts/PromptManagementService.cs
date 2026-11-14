using TriageIQ.Application.Abstractions;
using TriageIQ.Application.Abstractions.Persistence;
using TriageIQ.Application.Exceptions;
using TriageIQ.Domain.Entities;

namespace TriageIQ.Application.Prompts;

public sealed class PromptManagementService(
    IPromptVersionRepository prompts,
    IEvalRunRepository evalRuns,
    IClock clock)
{
    public async Task<IReadOnlyList<PromptVersionDto>> ListAsync(CancellationToken cancellationToken)
    {
        var all = await prompts.ListAllAsync(cancellationToken);
        return all.Select(Map).ToList();
    }

    public async Task<PromptVersionDto> CreateAsync(
        CreatePromptVersionRequest request,
        CancellationToken cancellationToken)
    {
        var entity = new PromptVersion
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Version = request.Version.Trim(),
            Template = request.Template,
            IsActive = false,
            CreatedAt = clock.UtcNow,
        };

        await prompts.AddAsync(entity, cancellationToken);
        return Map(entity);
    }

    public async Task ActivateAsync(Guid id, CancellationToken cancellationToken)
    {
        var prompt = await prompts.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Prompt version was not found.");

        await prompts.SetActiveAsync(prompt.Id, cancellationToken);
    }

    public async Task<PromptCompareResultDto?> CompareEvalRunsAsync(
        Guid leftRunId,
        Guid rightRunId,
        CancellationToken cancellationToken)
    {
        var left = await evalRuns.GetByIdAsync(leftRunId, cancellationToken);
        var right = await evalRuns.GetByIdAsync(rightRunId, cancellationToken);

        if (left is null || right is null)
        {
            return null;
        }

        return new PromptCompareResultDto(
            left.Id,
            right.Id,
            left.CategoryAccuracy,
            right.CategoryAccuracy,
            left.MacroF1,
            right.MacroF1);
    }

    private static PromptVersionDto Map(PromptVersion p) =>
        new(p.Id, p.Name, p.Version, p.Template, p.IsActive, p.CreatedAt);
}