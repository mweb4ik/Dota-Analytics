namespace DotaAnalytics.Api.Services;

public class MatchProcessingService
{
    private readonly MatchCacheService _cacheService;
    private readonly ILogger<MatchProcessingService> _logger;

    public MatchProcessingService(
        MatchCacheService cacheService,
        ILogger<MatchProcessingService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ProcessMatchResult> ProcessMatchAsync(long matchId)
    {
        var match = await _cacheService.GetMatchAsync(matchId);
        
        if (match == null)
            return ProcessMatchResult.NotFound();

        return ProcessMatchResult.Success(match);
    }
}

public record ProcessMatchResult(
    bool IsSuccess,
    bool IsNotFound,
    OpenDotaMatchResponse? Match = null
)
{
    public static ProcessMatchResult Success(OpenDotaMatchResponse match) =>
        new(true, false, match);

    public static ProcessMatchResult NotFound() =>
        new(false, true, null);
}