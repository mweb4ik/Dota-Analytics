using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;

namespace DotaAnalytics.Api.Endpoints;

public static class MatchPlayerStatsEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/matches/{matchId}/players", AddPlayerStatAsync);
    }

    /// <summary>
    /// Adds a new player statistic record to an existing match.
    /// Validates match existence, duplicate players, and match age before saving.
    /// </summary>
    /// <param name="matchId">The ID of the match to add the player to.</param>
    /// <param name="request">The player statistics data payload.</param>
    /// <param name="context">The database context.</param>
    /// <returns>201 Created if successful, 400 Bad Request for invalid ID, 404 Not Found if match doesn't exist, 
    /// 409 Conflict if player already exists, or 422 Unprocessable Entity if match is too old.</returns>
    private static async Task<IResult> AddPlayerStatAsync(
        long matchId,
        CreatePlayerStatRequest request,
        AppDbContext context)
    {
        var match = await context.Matches
            .Include(m => m.Players)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match is null)
            return Results.NotFound(new { error = "Match not found" });

        if (match.Players.Any(p => p.AccountId == request.AccountId))
        {
            return Results.Conflict(new
            {
                error = "Player already exists in this match",
                accountId = request.AccountId
            });
        }

        // Check if match ended more than 1 hour ago
        if (match.Duration > 0 && DateTime.UtcNow > match.StartTime.AddSeconds(match.Duration + 3600))
        {
            return Results.UnprocessableEntity(new
            {
                error = "Cannot add players to a completed match older than 1 hour"
            });
        }

        if (request.AccountId <= 0 || request.AccountId > 4294967295L)
            return Results.BadRequest(new { error = "Invalid Steam Account ID" });

        var playerStat = new MatchPlayerStat
        {
            MatchId = matchId,
            AccountId = request.AccountId,
            Kills = request.Kills,
            Deaths = request.Deaths,
            Assists = request.Assists,
            GoldPerMin = request.GoldPerMin,
            LastHits = request.LastHits,
            Hero = request.HeroId
        };

        context.MatchPlayerStats.Add(playerStat);
        await context.SaveChangesAsync();

        return Results.Created($"/api/matches/{matchId}/players/{playerStat.Id}", playerStat);
    }
}

/// <summary>
/// Represents the payload for adding a player's statistics to a match.
/// </summary>
/// <param name="AccountId">The 32-bit or 64-bit Steam Account ID of the player.</param>
/// <param name="Kills">Number of kills achieved by the player.</param>
/// <param name="Deaths">Number of deaths suffered by the player.</param>
/// <param name="Assists">Number of assists made by the player.</param>
/// <param name="GoldPerMin">Average gold earned per minute.</param>
/// <param name="LastHits">Total number of last hits.</param>
/// <param name="HeroId">The numeric ID of the hero played.</param>
public record CreatePlayerStatRequest(
    long AccountId,
    int Kills,
    int Deaths,
    int Assists,
    int GoldPerMin,
    int LastHits,
    Heroes HeroId
);