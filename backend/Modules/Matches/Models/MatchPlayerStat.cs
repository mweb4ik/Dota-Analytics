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
}