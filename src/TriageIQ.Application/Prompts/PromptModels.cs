namespace TriageIQ.Application.Prompts;

public sealed record PromptVersionDto(
    Guid Id,
    string Name,
    string Version,
    string Template,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record CreatePromptVersionRequest(
    string Name,
    string Version,
    string Template);

public sealed record PromptCompareResultDto(
    Guid LeftRunId,
    Guid RightRunId,
    decimal LeftCategoryAccuracy,
    decimal RightCategoryAccuracy,
    decimal LeftMacroF1,
    decimal RightMacroF1);