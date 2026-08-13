using DotaAnalytics.Api.Services;
using Microsoft.AspNetCore.Builder;

namespace DotaAnalytics.Api.Endpoints;

public static class FetchMatchEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/matches/fetch/{matchId}", FetchAndProcessMatchAsync);
    }

    /// <summary>
    /// Fetches match data from OpenDota API, saves it to the database, and caches it.
    /// Triggers the background processing pipeline for the specified match ID.
    /// </summary>
    /// <param name="matchId">The unique identifier of the Dota 2 match to fetch.</param>
    /// <param name="processingService">The service responsible for fetching and processing match data.</param>
    /// <returns>200 OK with a success message, or 404 Not Found if the match does not exist on OpenDota.</returns>
    private static async Task<IResult> FetchAndProcessMatchAsync(
        long matchId,
        MatchProcessingService processingService)
    {
        var result = await processingService.ProcessMatchAsync(matchId);

        return result.IsNotFound
            ? Results.NotFound(new { error = "Match not found" })
            : Results.Ok(new { message = $"Match {matchId} processed!" });
    }
}