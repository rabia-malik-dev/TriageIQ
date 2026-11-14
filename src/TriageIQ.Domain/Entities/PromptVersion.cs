namespace TriageIQ.Domain.Entities;

public sealed class PromptVersion
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Version { get; set; }

    public required string Template { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}