using DotaAnalytics.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;

namespace DotaAnalytics.Wpf.Services;

public class DotaAnalyticsService : IDotaAnalyticsService
{
    private readonly HttpClient _httpClient;

    public DotaAnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PaginatedResponse<OpenDotaMatchResponse>> GetMatchesAsync(int page,int pageSize)
    {
        try
        {
            var url = $"/api/matches?page={page}&pageSize={pageSize}";
            var response = await _httpClient.GetFromJsonAsync<PaginatedResponse<OpenDotaMatchResponse>>(url);

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
            var response = await _httpClient.GetFromJsonAsync<List<OpenDotaPlayerResponse>>(url);
            return response ?? new List<OpenDotaPlayerResponse>();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при запросе игроков матча {matchId}: {e.Message}");
            return new List<OpenDotaPlayerResponse>();
        }
    }
}