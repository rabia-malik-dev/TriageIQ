using TriageIQ.Application.Abstractions.Persistence;

namespace TriageIQ.Application.Analytics;

public sealed class AnalyticsQueryService(IAnalyticsRepository analytics)
{
    public Task<AnalyticsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken) =>
        analytics.GetSummaryAsync(cancellationToken);

    public Task<IReadOnlyList<AnalyticsTimeseriesPointDto>> GetTimeseriesAsync(
        int days,
        CancellationToken cancellationToken) =>
        analytics.GetTimeseriesAsync(days, cancellationToken);
}