using DotaAnalytics.Shared.Models;

namespace DotaAnalytics.Wpf.Services;

public interface IDotaAnalyticsService
{
    Task<PaginatedResponse<OpenDotaMatchResponse>> GetMatchesAsync(int page =1,int pageSize = 10);
    Task<List<OpenDotaPlayerResponse>> GetPlayersByMatchIdAsync(long matchId);
}