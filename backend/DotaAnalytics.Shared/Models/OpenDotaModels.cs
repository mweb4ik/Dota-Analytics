using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace DotaAnalytics.Shared.Models;

public class OpenDotaMatchResponse
{
    [JsonPropertyName("match_id")]
    public long MatchId { get; set; }

    [JsonPropertyName("id")]
    public long Id
    {
        get => MatchId;
        set => MatchId = value;
    }

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
    public long AccountId { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int GoldPerMin { get; set; }
    public int LastHits { get; set; }
    public int HeroId { get; set; }
    public int XpPerMin { get; set; }
    public int HeroDamage { get; set; }
    public int TowerDamage { get; set; }
    public int HeroHealing { get; set; }
    public int Denies { get; set; }
    public int Level { get; set; }
    public int NetWorth { get; set; }
    public double LaneEfficiency { get; set; }
}