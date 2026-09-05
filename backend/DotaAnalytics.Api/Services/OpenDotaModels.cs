using System.Text.Json.Serialization;

namespace DotaAnalytics.Api.Services;

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


    [JsonPropertyName("xp_per_min")]
    public int XpPerMin { get; set; }

    [JsonPropertyName("hero_damage")]
    public int HeroDamage { get; set; }

    [JsonPropertyName("tower_damage")]
    public int TowerDamage { get; set; }

    [JsonPropertyName("hero_healing")]
    public int HeroHealing { get; set; }

    [JsonPropertyName("denies")]
    public int Denies { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("net_worth")]
    public int NetWorth { get; set; }

    [JsonPropertyName("lane_efficiency")]
    public double LaneEfficiency { get; set; }
}