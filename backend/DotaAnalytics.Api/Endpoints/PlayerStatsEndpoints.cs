using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;
namespace DotaAnalytics.Api.Endpoints;

public static class PlayerStatsEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/players/{accountId}/stats", GetPlayerSummaryStatsAsync);
        app.MapGet("/api/players/{accountId}/heroes", GetHeroSummaryStatsAsync);

        // Глобальный лидерборд (без accountId)
        app.MapGet("/api/leaderboards", GetLeaderboardStatsAsync);
    }

    /// <summary>
    /// Retrieves aggregated statistics for a specific player across their recent matches.
    /// Calculates win rate, average K/D/A, and most played hero based on the last 20 matches.
    /// </summary>
    private static async Task<IResult> GetPlayerSummaryStatsAsync(
        long accountId,
        AppDbContext context)
    {
        var recentMatches = await context.Matches
            .Include(m => m.Players)
            .Where(m => m.Players.Any(p => p.AccountId == accountId))
            .OrderByDescending(m => m.StartTime)
            .Take(20)
            .ToListAsync();

        if (!recentMatches.Any())
        {
            return Results.NotFound(new { error = "No matches found for this player", accountId });
        }

        var playerStats = recentMatches
            .SelectMany(m => m.Players)
            .Where(p => p.AccountId == accountId)
            .ToList();

        int totalMatches = playerStats.Count;
        int wins = 0;

        foreach (var stat in playerStats)
        {
            var match = recentMatches.First(m => m.Id == stat.MatchId);
            bool isPlayerRadiant = IsRadiantSide(stat.Hero);

            if ((match.RadiantWin && isPlayerRadiant) || (!match.RadiantWin && !isPlayerRadiant))
            {
                wins++;
            }
        }

        double winRate = totalMatches > 0 ? (double)wins / totalMatches * 100 : 0;
        double avgKills = playerStats.Average(p => p.Kills);
        double avgDeaths = playerStats.Average(p => p.Deaths);
        double avgAssists = playerStats.Average(p => p.Assists);
        double kda = avgDeaths > 0 ? (avgKills + avgAssists) / avgDeaths : avgKills + avgAssists;

        var topHero = playerStats
            .GroupBy(p => p.Hero)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return Results.Ok(new
        {
            accountId,
            totalMatches,
            wins,
            losses = totalMatches - wins,
            winRate = Math.Round(winRate, 2),
            averageKills = Math.Round(avgKills, 2),
            averageDeaths = Math.Round(avgDeaths, 2),
            averageAssists = Math.Round(avgAssists, 2),
            kdaRatio = Math.Round(kda, 2),
            mostPlayedHeroId = topHero?.Key,
            mostPlayedHeroGames = topHero?.Count() ?? 0
        });
    }

    /// <summary>
    /// Retrieves per-hero statistics for a specific player across their recent matches.
    /// Returns win rate, games played, and average KDA for each hero.
    /// </summary>
    private static async Task<IResult> GetHeroSummaryStatsAsync(
        long accountId,
        AppDbContext context)
    {
        var recentMatches = await context.Matches
            .Include(m => m.Players)
            .Where(m => m.Players.Any(p => p.AccountId == accountId))
            .OrderByDescending(m => m.StartTime)
            .Take(50)
            .ToListAsync();

        if (!recentMatches.Any())
        {
            return Results.NotFound(new { error = "No matches found for this player", accountId });
        }

        var playerStats = recentMatches
            .SelectMany(m => m.Players)
            .Where(p => p.AccountId == accountId)
            .ToList();

        var heroStats = playerStats
            .GroupBy(p => p.Hero)
            .Select(group =>
            {
                var heroId = group.Key;
                var stats = group.ToList();
                int gamesPlayed = stats.Count;
                int wins = 0;

                foreach (var stat in stats)
                {
                    var match = recentMatches.First(m => m.Id == stat.MatchId);
                    bool isPlayerRadiant = IsRadiantSide(stat.Hero);

                    if ((match.RadiantWin && isPlayerRadiant) || (!match.RadiantWin && !isPlayerRadiant))
                    {
                        wins++;
                    }
                }

                double winRate = (double)wins / gamesPlayed * 100;
                double avgKills = stats.Average(p => p.Kills);
                double avgDeaths = stats.Average(p => p.Deaths);
                double avgAssists = stats.Average(p => p.Assists);
                double kda = avgDeaths > 0 ? (avgKills + avgAssists) / avgDeaths : avgKills + avgAssists;

                return new
                {
                    heroId = (int)heroId,
                    heroName = heroId.ToString(),
                    gamesPlayed,
                    wins,
                    losses = gamesPlayed - wins,
                    winRate = Math.Round(winRate, 2),
                    averageKills = Math.Round(avgKills, 2),
                    averageDeaths = Math.Round(avgDeaths, 2),
                    averageAssists = Math.Round(avgAssists, 2),
                    kdaRatio = Math.Round(kda, 2)
                };
            })
            .OrderByDescending(h => h.gamesPlayed)
            .ToList();

        return Results.Ok(new
        {
            accountId,
            totalHeroesPlayed = heroStats.Count,
            heroes = heroStats
        });
    }

    /// <summary>
    /// Retrieves the global leaderboard of top players by win rate.
    /// Filters out players with less than 10 matches to ensure statistical relevance.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <returns>200 OK with a list of top 10 players.</returns>
    private static async Task<IResult> GetLeaderboardStatsAsync(AppDbContext context)
    {

        var allMatches = await context.Matches
            .Include(m => m.Players)
            .ToListAsync();

        var allPlayerStats = allMatches
            .SelectMany(m => m.Players)
            .ToList();

        var leaderboard = allPlayerStats
            .GroupBy(p => p.AccountId)
            .Select(group =>
            {
                var accountId = group.Key;
                var stats = group.ToList();
                int totalMatches = stats.Count;
                int wins = 0;

                foreach (var stat in stats)
                {

                    var match = allMatches.First(m => m.Id == stat.MatchId);
                    bool isPlayerRadiant = IsRadiantSide(stat.Hero);

                    if ((match.RadiantWin && isPlayerRadiant) || (!match.RadiantWin && !isPlayerRadiant))
                    {
                        wins++;
                    }
                }

                double winRate = totalMatches > 0 ? (double)wins / totalMatches * 100 : 0;

                return new
                {
                    accountId,
                    totalMatches,
                    wins,
                    losses = totalMatches - wins,
                    winRate = Math.Round(winRate, 2)
                };
            })
            .Where(x => x.totalMatches >= 10) 
            .OrderByDescending(x => x.winRate) 
            .ThenByDescending(x => x.totalMatches) 
            .Take(10) 
            .ToList();

        return Results.Ok(new
        {
            totalPlayersEvaluated = leaderboard.Count,
            topPlayers = leaderboard
        });
    }

    /// <summary>
    /// Helper method to determine player's side based on hero ID (placeholder logic).
    /// Even ID = Radiant, Odd ID = Dire.
    /// </summary>
    private static bool IsRadiantSide(Heroes hero)
    {
        return (int)hero % 2 == 0;
    }
}