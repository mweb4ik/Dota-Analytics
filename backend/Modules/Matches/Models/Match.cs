namespace DotaAnalytics.Modules.Matches.Models;

public class Match
{
    public long Id {get;set;}
    public int Duration {get;set;}
    public bool RadiantWin {get;set;}
    public DateTime StartTime {get;set;}
    public DateTime CachedAt {get;set;}

    public ICollection<MatchPlayerStat> Players { get; set; } = new List<MatchPlayerStat>();
}
