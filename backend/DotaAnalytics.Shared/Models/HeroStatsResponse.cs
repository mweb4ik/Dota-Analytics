using System.Text.Json.Serialization;

namespace DotaAnalytics.Shared.Models;

public class HeroStat
{
    [JsonPropertyName("heroId")]
    public int HeroId { get; set; }

    [JsonPropertyName("gamesPlayed")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("wins")]
    public int Wins { get; set; }

    [JsonPropertyName("losses")]
    public int Losses { get; set; }

    [JsonPropertyName("winRate")]
    public double WinRate { get; set; }

    [JsonPropertyName("averageKills")]
    public double AverageKills { get; set; }

    [JsonPropertyName("averageDeaths")]
    public double AverageDeaths { get; set; }

    [JsonPropertyName("averageAssists")]
    public double AverageAssists { get; set; }
}