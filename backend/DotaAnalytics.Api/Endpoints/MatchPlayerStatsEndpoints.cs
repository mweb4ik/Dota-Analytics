using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models;

namespace DotaAnalytics.Api.Endpoints;

public static class MatchPlayerStatsEndpoints
{
    public static void Map(WebApplication app)
    {
       

app.MapPost("/api/matches/{matchId}/players", async (
    long matchId,
    CreatePlayerStatRequest request,
    AppDbContext context) =>
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

    if (match.Duration > 0 && DateTime.UtcNow > match.StartTime.AddSeconds(match.Duration + 3600))
    {
        return Results.UnprocessableEntity(new 
        { 
            error = "Cannot add players to a completed match older than 1 hour" 
        });
    }
    if (request.AccountId <= 0 || request.AccountId > 4294967295L)
    return Results.BadRequest(new { error = "Invalid Steam Account ID" });
    var playerStat = new MatchPlayerStat {  
                MatchId = matchId,
                AccountId = request.AccountId,
                Kills = request.Kills,
                Deaths = request.Deaths,
                Assists = request.Assists,
                GoldPerMin = request.GoldPerMin,
                LastHits = request.LastHits,
                Hero = request.HeroId, };
    context.MatchPlayerStats.Add(playerStat);
    await context.SaveChangesAsync();
    return Results.Created($"/api/matches/{matchId}/players/{playerStat.Id}", playerStat);
});
    }
}


public record CreatePlayerStatRequest(
    long AccountId,
    int Kills,
    int Deaths,
    int Assists,
    int GoldPerMin,
    int LastHits,
    Heroes HeroId
);