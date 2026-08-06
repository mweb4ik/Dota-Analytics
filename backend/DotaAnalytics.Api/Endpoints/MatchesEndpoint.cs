using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;
namespace DotaAnalytics.Api.Endpoints;

public static class MatchesEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/matches", async (
            long? accountId,
            int page,
            int pageSize,
            bool? radiantWin,
            string? sortBy,
            AppDbContext context) =>
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
                query = query.Where(m => 
                    m.Players.Any(p => p.AccountId == accountId.Value)
                );
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
        });

        app.MapPost("/api/matches", async (CreateMatchRequest request, AppDbContext context) =>
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
        });
    }
}

public record CreateMatchRequest(int Duration, bool RadiantWin, DateTime StartTime);