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
            Console.WriteLine($"Error requesting the API: {e.Message}");
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
            Console.WriteLine($"Error retrieving match players {matchId}: {e.Message}");
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
            Console.WriteLine($"Error requesting hero statistics: {e.Message}");
            return new List<HeroStat>();
        }
    }
    public async Task<string> ClearDatabaseAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/refresh/clear", null);
            if (response.IsSuccessStatusCode)
            {
                return "Database and cache is clear!";
            }
            return $"Error: {(int)response.StatusCode} {response.ReasonPhrase}";
        }
        catch (HttpRequestException e)
        {
            return $"Network error: {e.Message}";
        }
    }

    public async Task<string> FetchFreshDataAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/api/refresh/fetch", null);
            if (response.IsSuccessStatusCode)
            {
                return " Data successfully loaded from the OpenDota API";
            }
            return $" Error: {(int)response.StatusCode} {response.ReasonPhrase}";
        }
        catch (HttpRequestException e)
        {
            return $"Network error: {e.Message}";
        }
    }
}