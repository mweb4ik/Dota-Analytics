using System.Text.Json.Serialization;

namespace DotaAnalytics.Api.Services;

// Главный класс для ответа по матчу
public class OpenDotaMatchResponse
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("radiant_win")]
    public bool RadiantWin { get; set; }

    [JsonPropertyName("start_time")]
    public long StartTimeUnix { get; set; } 

    [JsonPropertyName("players")]
    public List<OpenDotaPlayerResponse> Players { get; set; } = new();
}

public class OpenDotaPlayerResponse
{
    [JsonPropertyName("account_id")]
    public long AccountId { get; set; }

    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int Assists { get; set; }

    [JsonPropertyName("gold_per_min")]
    public int GoldPerMin { get; set; }

    [JsonPropertyName("last_hits")]
    public int LastHits { get; set; }

    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; } 
}