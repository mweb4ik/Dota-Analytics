using System.Text.Json;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Text.Json.Serialization;
namespace DotaAnalytics.Api.Services;
public class MatchCacheService
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly AppDbContext _context;
    private readonly ILogger<MatchCacheService> _logger;

    private const string MATCH_CACHE_PREFIX = "match_";
    private const string PRO_MATCHES_KEY = "pro_matches_list";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public MatchCacheService(
        IMemoryCache cache,
        HttpClient httpClient,
        AppDbContext context,
        ILogger<MatchCacheService> logger)
    {
        _cache = cache;
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
    }

    public async Task<OpenDotaMatchResponse?> GetMatchAsync(long matchId)
    {
        var cacheKey = $"{MATCH_CACHE_PREFIX}{matchId}";
        if (_cache.TryGetValue(cacheKey, out OpenDotaMatchResponse? cachedMatch))
        {
            _logger.LogInformation("Матч {MatchId} найден в кэше", matchId);
            return cachedMatch;
        }

        var dbMatch = await _context.Matches
            .Include(m => m.Players)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (dbMatch != null)
        {
            var dto = ConvertToDto(dbMatch);
            _cache.Set(cacheKey, dto, CacheDuration);
            _logger.LogInformation("Матч {MatchId} загружен из БД и закэширован", matchId);
            return dto;
        }

        try
        {
            var response = await _httpClient.GetAsync($"https://api.opendota.com/api/matches/{matchId}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var match = JsonSerializer.Deserialize<OpenDotaMatchResponse>(json, new JsonSerializerOptions 
            { PropertyNameCaseInsensitive = true });

            if (match != null)
            {
                await SaveMatchToDbAsync(match);

                _cache.Set(cacheKey, match, CacheDuration);
                _logger.LogInformation("Матч {MatchId} загружен из OpenDota и закэширован", matchId);
            }

            return match;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки матча {MatchId}", matchId);
            return null;
        }
    }

    public async Task PreloadProMatchesAsync()
    {
        try
        {
            _logger.LogInformation("Начинаю предзагрузку про-матчей...");
            
            var response = await _httpClient.GetAsync("https://api.opendota.com/api/proMatches");
            if (!response.IsSuccessStatusCode) return;

            var json = await response.Content.ReadAsStringAsync();
            var proMatches = JsonSerializer.Deserialize<List<ProMatchSummary>>(json, new JsonSerializerOptions 
            { PropertyNameCaseInsensitive = true });

            if (proMatches == null || !proMatches.Any()) return;

            foreach (var m in proMatches.Take(15))
{
    try
    {
        await GetMatchAsync(m.MatchId);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Не удалось загрузить про-матч {MatchId}", m.MatchId);
    }
    
    await Task.Delay(1100); 
}
            
            _cache.Set(PRO_MATCHES_KEY, proMatches.Take(15).Select(m => m.MatchId).ToList(), TimeSpan.FromHours(1));
            
            _logger.LogInformation("Предзагрузка завершена. Закэшировано {Count} про-матчей", proMatches.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка предзагрузки про-матчей");
        }
    }

    private OpenDotaMatchResponse ConvertToDto(Match dbMatch)
    {
        return new OpenDotaMatchResponse
        {
            MatchId = dbMatch.Id,
            Duration = dbMatch.Duration,
            RadiantWin = dbMatch.RadiantWin,
            StartTimeUnix = new DateTimeOffset(dbMatch.StartTime).ToUnixTimeSeconds(),
            Players = dbMatch.Players.Select(p => new OpenDotaPlayerResponse
            {
                AccountId = p.AccountId,
                Kills = p.Kills,
                Deaths = p.Deaths,
                Assists = p.Assists,
                GoldPerMin = p.GoldPerMin,
                LastHits = p.LastHits,
                HeroId = (int)p.Hero
            }).ToList()
        };
    }

    private async Task SaveMatchToDbAsync(OpenDotaMatchResponse match)
    {
     
    if (await _context.Matches.AnyAsync(m => m.Id == match.MatchId)) 
    {
        _logger.LogDebug("Матч {MatchId} уже существует в БД, пропуск сохранения", match.MatchId);
        return; 
    }
        var entity = new Match
        {
            Id = match.MatchId,
            Duration = match.Duration,
            RadiantWin = match.RadiantWin,
            StartTime = DateTimeOffset.FromUnixTimeSeconds(match.StartTimeUnix).UtcDateTime,
            CachedAt = DateTime.UtcNow,
            Players = match.Players.Select(p => new MatchPlayerStat
            {
                AccountId = p.AccountId,
                Kills = p.Kills,
                Deaths = p.Deaths,
                Assists = p.Assists,
                GoldPerMin = p.GoldPerMin,
                LastHits = p.LastHits,
                Hero = (Heroes)p.HeroId
            }).ToList()
        };

        await _context.Matches.AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
public class ProMatchSummary
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }
}