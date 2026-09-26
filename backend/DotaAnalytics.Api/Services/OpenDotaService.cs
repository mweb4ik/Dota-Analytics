    using System.Text.Json;
    using DotaAnalytics.Infrastructure.Persistence;
    using DotaAnalytics.Modules.Matches.Models;
    using Microsoft.EntityFrameworkCore;
    using DotaAnalytics.Shared.Models;
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

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var match = new Match
        {
            Id = root.GetProperty("match_id").GetInt64(),
            Duration = root.GetProperty("duration").GetInt32(),
            RadiantWin = root.GetProperty("radiant_win").GetBoolean(),
            StartTime = DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("start_time").GetInt64()).UtcDateTime,
            CachedAt = DateTime.UtcNow
        };

        var playerStats = new List<MatchPlayerStat>();

        if (root.TryGetProperty("players", out var playersElement) && playersElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var p in playersElement.EnumerateArray())
            {
                int GetInt(string name) => p.TryGetProperty(name, out var el) ? el.GetInt32() : 0;
                long GetLong(string name) => p.TryGetProperty(name, out var el) ? el.GetInt64() : 0;
                double GetDouble(string name) => p.TryGetProperty(name, out var el) ? el.GetDouble() : 0.0;

                playerStats.Add(new MatchPlayerStat
                {
                    MatchId = match.Id,
                    AccountId = GetLong("account_id"),
                    Kills = GetInt("kills"),
                    Deaths = GetInt("deaths"),
                    Assists = GetInt("assists"),
                    GoldPerMin = GetInt("gold_per_min"),
                    XpPerMin = GetInt("xp_per_min"),
                    LastHits = GetInt("last_hits"),
                    Denies = GetInt("denies"),
                    Level = GetInt("level"),
                    Hero = (Heroes)GetInt("hero_id"),
                    HeroDamage = GetInt("hero_damage"),
                    TowerDamage = GetInt("tower_damage"),
                    HeroHealing = GetInt("hero_healing"),
                    LaneEfficiency = GetDouble("lane_efficiency"),
                    NetWorth = GetInt("net_worth")
                });
            }
        }

        await _context.Matches.AddAsync(match);
        await _context.MatchPlayerStats.AddRangeAsync(playerStats);
        await _context.SaveChangesAsync();

        return true;
    }
}