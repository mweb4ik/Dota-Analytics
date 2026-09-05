using System.Text.Json.Serialization;

namespace DotaAnalytics.Modules.Matches.Models;

public class MatchPlayerStat {
public long Id {get;set;}
public long MatchId {get;set;}
public long AccountId {get;set;}
public int Kills {get;set;}
public int Deaths {get;set;}
public int Assists {get;set;}
public int GoldPerMin {get;set;}
public int LastHits {get;set;}
public Heroes Hero { get; set; }

    public int XpPerMin { get; set; }
    public int HeroDamage { get; set; }
    public int TowerDamage { get; set; }
    public int HeroHealing { get; set; }
    public int Denies { get; set; }
    public int Level { get; set; }
    public int NetWorth { get; set; }
    public double LaneEfficiency { get; set; }

    [JsonIgnore]
    public Match Match { get; set; }
}