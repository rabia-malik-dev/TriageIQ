using TriageIQ.Application.Abstractions;

namespace TriageIQ.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}