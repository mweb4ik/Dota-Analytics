using DotaAnalytics.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace DotaAnalytics.Wpf.Services;

public class DotaAnalyticsService : IDotaAnalyticsService
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };
    public DotaAnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedResponse<OpenDotaMatchResponse>> GetMatchesAsync(int page,int pageSize)
    {
        try
        {
            var url = $"/api/matches?page={page}&pageSize={pageSize}";
            var response = await _httpClient.GetFromJsonAsync<PaginatedResponse<OpenDotaMatchResponse>>(url, _jsonOptions);

            return response ?? new PaginatedResponse<OpenDotaMatchResponse>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при запросе к API: {e.Message}");
            return new PaginatedResponse<OpenDotaMatchResponse>();
        }
    }
    public async Task<List<OpenDotaPlayerResponse>> GetPlayersByMatchIdAsync(long matchId)
    {
        try
        {
            var url = $"/api/matches/{matchId}/players";
            var response = await _httpClient.GetFromJsonAsync<List<OpenDotaPlayerResponse>>(url, _jsonOptions);
            return response ?? new List<OpenDotaPlayerResponse>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при запросе игроков матча {matchId}: {e.Message}");
            return new List<OpenDotaPlayerResponse>();
        }
    }
    public async Task<LeaderboardResponse> GetLeaderboardAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<LeaderboardResponse>("/api/leaderboards", _jsonOptions);
            return response ?? new LeaderboardResponse();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при запросе лидерборда: {e.Message}");
            return new LeaderboardResponse();
        }
    }
    public async Task<List<HeroStat>> GetHeroStatsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<HeroStat>>("/api/heroes/stats", _jsonOptions);
            return response ?? new List<HeroStat>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при запросе статистики героев: {e.Message}");
            return new List<HeroStat>();
        }
    }
}