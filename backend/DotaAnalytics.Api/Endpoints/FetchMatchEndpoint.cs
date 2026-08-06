using DotaAnalytics.Api.Services;
using Microsoft.AspNetCore.Builder;

namespace DotaAnalytics.Api.Endpoints;

public static class FetchMatchEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/matches/fetch/{matchId}", async (
            long matchId, 
            MatchProcessingService processingService) => 
        {
            var result = await processingService.ProcessMatchAsync(matchId);
            
            return result.IsNotFound 
                ? Results.NotFound(new { error = "Match not found" })
                : Results.Ok(new { message = $"Match {matchId} processed!" });
        });
    }
}