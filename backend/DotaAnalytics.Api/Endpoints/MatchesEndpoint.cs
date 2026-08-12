using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;

namespace DotaAnalytics.Api.Endpoints;

public static class MatchesEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/matches", GetMatchesAsync);

        app.MapPost("/api/matches", CreateMatchAsync);

    }

    /// <summary>
    /// Retrieves a paginated list of matches with optional filtering and sorting.
    /// </summary>
    /// <param name="accountId">The 32-bit or 64-bit Steam ID of the player to filter by (optional).</param>
    /// <param name="page">The page number for pagination (default: 1).</param>
    /// <param name="pageSize">The number of records per page (default: 10, max: 100).</param>
    /// <param name="radiantWin">Filters matches by the winning team (true for Radiant, false for Dire).</param>
    /// <param name="sortBy">The attribute to sort by: "duration", "starttime", "radiant_win", or defaults to ID.</param>
    /// <param name="context">The database context.</param>
    /// <returns>A paginated response containing the matches and total count metadata.</returns>
    private static async Task<IResult> GetMatchesAsync(
        long? accountId,
        int page,
        int pageSize,
        bool? radiantWin,
        string? sortBy,
        AppDbContext context)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        IQueryable<Match> query = context.Matches;

        if (radiantWin.HasValue)
        {
            query = query.Where(m => m.RadiantWin == radiantWin.Value);
        }

        if (accountId.HasValue)
        {
            query = query.Where(m => m.Players.Any(p => p.AccountId == accountId.Value));
        }

        query = sortBy?.ToLower() switch
        {
            "duration" => query.OrderByDescending(m => m.Duration),
            "starttime" => query.OrderByDescending(m => m.StartTime),
            "radiant_win" => query.OrderByDescending(m => m.RadiantWin),
            _ => query.OrderByDescending(m => m.Id)
        };

        int totalCount = await query.CountAsync();
        int skip = (page - 1) * pageSize;

        var matches = await query
            .Include(m => m.Players)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return Results.Ok(new
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Items = matches
        });
    }

    /// <summary>
    /// Creates a new basic match record in the database.
    /// </summary>
    /// <param name="request">The match creation data payload.</param>
    /// <param name="context">The database context.</param>
    /// <returns>A 201 Created response with the newly created match data.</returns>
    private static async Task<IResult> CreateMatchAsync(CreateMatchRequest request, AppDbContext context)
    {
        var match = new Match
        {
            Duration = request.Duration,
            RadiantWin = request.RadiantWin,
            StartTime = request.StartTime,
            CachedAt = DateTime.UtcNow
        };

        context.Matches.Add(match);
        await context.SaveChangesAsync();

        return Results.Created($"/api/matches/{match.Id}", match);
    }
}

/// <summary>
/// Represents the payload for creating a new match.
/// </summary>
/// <param name="Duration">The duration of the match in seconds.</param>
/// <param name="RadiantWin">Indicates whether the Radiant team won.</param>
/// <param name="StartTime">The UTC time when the match started.</param>
public record CreateMatchRequest(int Duration, bool RadiantWin, DateTime StartTime);