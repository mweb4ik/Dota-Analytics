using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Api.Services;
using Microsoft.Extensions.Caching.Memory;

namespace DotaAnalytics.Api.Endpoints;

/// <summary>
/// Endpoints for managing application data.
/// Provides functionality to clear the database and fetch fresh data from the OpenDota API.
/// Recommended for use primarily in development or maintenance scenarios.
/// </summary>
public static class RefreshEndpoints
{
    /// <summary>
    /// Registers all data management endpoints with the provided web application.
    /// </summary>
    /// <param name="app">The web application instance.</param>
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/refresh/clear", ClearDatabaseAsync)
            .WithTags("Data Management")
            .WithSummary("Clear all data from the database and cache")
            .WithDescription("Removes all matches and player statistics from the database and flushes the in-memory cache. Use this before fetching fresh data.");

        app.MapPost("/api/refresh/fetch", FetchFreshDataAsync)
            .WithTags("Data Management")
            .WithSummary("Fetch fresh data from the OpenDota API")
            .WithDescription("Fetches the latest 50 professional matches with detailed player statistics from the OpenDota API. This process typically takes 30-60 seconds.");
    }

    /// <summary>
    /// Clears all match and player statistics data from the database and flushes the in-memory cache.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="memoryCache">The application's in-memory cache.</param>
    /// <returns>An HTTP result indicating the outcome of the clear operation.</returns>
    /// <response code="200">Database and cache successfully cleared.</response>
    /// <response code="500">An error occurred while clearing the data.</response>
    private static async Task<IResult> ClearDatabaseAsync(
        AppDbContext context,
        IMemoryCache memoryCache)
    {
        try
        {
            Console.WriteLine("[REFRESH:CLEAR] Starting database cleanup...");

            context.MatchPlayerStats.RemoveRange(context.MatchPlayerStats);

            context.Matches.RemoveRange(context.Matches);

            await context.SaveChangesAsync();
            Console.WriteLine("[REFRESH:CLEAR] Database cleared.");

            if (memoryCache is MemoryCache concreteCache)
            {
                concreteCache.Compact(1.0);
                Console.WriteLine("[REFRESH:CLEAR] In-memory cache cleared.");
            }

            Console.WriteLine("[REFRESH:CLEAR] Operation completed successfully.");
            return Results.Ok(new
            {
                message = "Database and cache successfully cleared",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REFRESH:CLEAR ERROR] {ex.Message}");
            return Results.Problem(
                detail: $"Error clearing data: {ex.Message}",
                statusCode: 500
            );
        }
    }

    /// <summary>
    /// Fetches fresh professional matches from the OpenDota API, including detailed player statistics.
    /// Fetches the latest 50 matches, which typically takes 30-60 seconds to complete.
    /// </summary>
    /// <param name="cacheService">The match caching service responsible for fetching and saving data.</param>
    /// <returns>An HTTP result indicating the outcome of the fetch operation.</returns>
    /// <response code="200">Data successfully fetched and saved from OpenDota.</response>
    /// <response code="500">An error occurred while fetching data.</response>
    /// <remarks>
    /// It is highly recommended to call <c>/api/refresh/clear</c> first to remove stale data 
    /// before invoking this endpoint to fetch fresh data.
    /// </remarks>
    private static async Task<IResult> FetchFreshDataAsync(MatchCacheService cacheService)
    {
        try
        {
            Console.WriteLine("[REFRESH:FETCH] Starting fresh data fetch from OpenDota...");
            Console.WriteLine("[REFRESH:FETCH] This may take 30-60 seconds, please wait...");

            await cacheService.PreloadProMatchesAsync();

            Console.WriteLine("[REFRESH:FETCH] Data successfully fetched and saved to DB!");
            return Results.Ok(new
            {
                message = "Data successfully fetched from OpenDota API",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[REFRESH:FETCH ERROR] {ex.Message}");
            return Results.Problem(
                detail: $"Error fetching data: {ex.Message}",
                statusCode: 500
            );
        }
    }
}