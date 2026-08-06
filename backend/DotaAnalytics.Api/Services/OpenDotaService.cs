using System.Text.Json;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;
using Microsoft.EntityFrameworkCore;
namespace DotaAnalytics.Api.Services;

public class OpenDotaService
{
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;

    public OpenDotaService(HttpClient httpClient, AppDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<bool> FetchAndSaveMatchAsync(long matchId)
    {
        if (await _context.Matches.AnyAsync(m => m.Id == matchId))
        {
            return false;
        }

        var response = await _httpClient.GetAsync($"https://api.opendota.com/api/matches/{matchId}");
        
        if (!response.IsSuccessStatusCode)
        {
           throw new Exception($"Не удалось получить матч {matchId}. Статус: {response.StatusCode}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var openDotaMatch = JsonSerializer.Deserialize<OpenDotaMatchResponse>(json, new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true 
        });

        if (openDotaMatch == null)
        {
            throw new Exception("Не удалось распарсить ответ от OpenDota");
        }

        var match = new Match
        {
            Id = openDotaMatch.MatchId,
            Duration = openDotaMatch.Duration,
            RadiantWin = openDotaMatch.RadiantWin,
            StartTime = DateTimeOffset.FromUnixTimeSeconds(openDotaMatch.StartTimeUnix).UtcDateTime,
            CachedAt = DateTime.UtcNow
        };

        var playerStats = openDotaMatch.Players.Select(p => new MatchPlayerStat
        {
            MatchId = match.Id,
            AccountId = p.AccountId,
            Kills = p.Kills,
            Deaths = p.Deaths,
            Assists = p.Assists,
            GoldPerMin = p.GoldPerMin,
            LastHits = p.LastHits,
            Hero = (Heroes)p.HeroId,
        }).ToList();

        await _context.Matches.AddAsync(match);
        await _context.MatchPlayerStats.AddRangeAsync(playerStats);
        await _context.SaveChangesAsync();

        return true;
    }
}