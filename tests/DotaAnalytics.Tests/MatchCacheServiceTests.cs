using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using Xunit;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models; 
using DotaAnalytics.Api.Services;

namespace DotaAnalytics.Tests;

public class MatchCacheServiceTests
{
    private readonly IMemoryCache _realCache;
    private readonly AppDbContext _inMemoryDbContext;
    private readonly Mock<HttpClient> _mockHttpClient;
    private readonly Mock<ILogger<MatchCacheService>> _mockLogger;
    private readonly MatchCacheService _service;

    public MatchCacheServiceTests()
    {
        _realCache = new MemoryCache(new MemoryCacheOptions());

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _inMemoryDbContext = new AppDbContext(options);

        _mockHttpClient = new Mock<HttpClient>();

        _mockLogger = new Mock<ILogger<MatchCacheService>>();

        _service = new MatchCacheService(
            _realCache,
            _mockHttpClient.Object,
            _inMemoryDbContext,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task GetMatchAsync_WhenMatchExistsInCache_ShouldReturnMatch()
    {
        // Arrange
        var matchId = 123456789L;
        var expectedMatch = new OpenDotaMatchResponse 
        {
            MatchId = matchId, 
            RadiantWin = true,
            Duration = 2500
        };

        _realCache.Set($"match_{matchId}", expectedMatch);

        // Act
        var result = await _service.GetMatchAsync(matchId);

        // Assert
        result.Should().NotBeNull("Match was cached, so it should be returned");
        result!.MatchId.Should().Be(matchId);
        result.RadiantWin.Should().BeTrue();
    }

    [Fact]
    public async Task GetMatchAsync_WhenMatchNotInCache_ShouldQueryDatabase()
    {
        // Arrange
        var matchId = 999999999L;

        // Act
        var result = await _service.GetMatchAsync(matchId);

        // Assert
        result.Should().BeNull("The match is not in cache or database");
    }

    [Fact]
    public async Task PreloadProMatchesAsync_ShouldNotDuplicateExistingMatches()
    {
        // Arrange
        var existingMatchId = 111111111L;

        var existingMatch = new DotaAnalytics.Modules.Matches.Models.Match
        {
            Id = existingMatchId,
            RadiantWin = true,
            Duration = 2000,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow
        };

        _inMemoryDbContext.Matches.Add(existingMatch);
        await _inMemoryDbContext.SaveChangesAsync();

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,   
                Content = new StringContent("[]")
            });

        var httpClient = new HttpClient(mockHandler.Object);
        var serviceWithMockHttp = new MatchCacheService(
            _realCache,
            httpClient,
            _inMemoryDbContext,
            _mockLogger.Object
        );

        // Act
        await serviceWithMockHttp.PreloadProMatchesAsync();

        // Assert
        var count = await _inMemoryDbContext.Matches.CountAsync(x => x.Id == existingMatchId);
        count.Should().Be(1, "Service must not create duplicates of existing matches");
    }
}