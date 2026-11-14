using TriageIQ.Application.Analytics;

namespace TriageIQ.Application.Abstractions.Persistence;

public interface IAnalyticsRepository
{
    Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AnalyticsTimeseriesPointDto>> GetTimeseriesAsync(
        int days,
        CancellationToken cancellationToken);
}