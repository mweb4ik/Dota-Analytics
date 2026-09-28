using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.Json.Serialization;

namespace DotaAnalytics.Shared.Models;

public class LeaderboardResponse
{
    [JsonPropertyName("totalPlayersEvaluated")]
    public int TotalPlayersEvaluated { get; set; }

    [JsonPropertyName("topPlayers")]
    public List<PlayerLeaderboardStat> TopPlayers { get; set; } = new();
}

public class PlayerLeaderboardStat
{
    [JsonPropertyName("accountId")]
    public long AccountId { get; set; }

    [JsonPropertyName("totalMatches")]
    public int TotalMatches { get; set; }

    [JsonPropertyName("wins")]
    public int Wins { get; set; }

    [JsonPropertyName("losses")]
    public int Losses { get; set; }

    [JsonPropertyName("winRate")]
    public double WinRate { get; set; }
}
