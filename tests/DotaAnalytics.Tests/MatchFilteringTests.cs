using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Modules.Matches.Models; 

namespace DotaAnalytics.Tests;

public class MatchFilteringTests
{
    private readonly AppDbContext _inMemoryDbContext;

    public MatchFilteringTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _inMemoryDbContext = new AppDbContext(options);
    }

    [Fact]
    public async Task FilterByAccountId_ShouldReturnOnlyMatchesWithSpecificPlayer()
    {
        // Arrange
        var targetAccountId = 123456789L;
        var otherAccountId = 987654321L;

        var match1 = new Match
        {
            Id = 1111111L,
            Duration = 2500,
            RadiantWin = true,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat>
            {
                new MatchPlayerStat { AccountId = targetAccountId, Kills = 10 }
            }
        };

        var match2 = new Match
        {
            Id = 2222222L,
            Duration = 3000,
            RadiantWin = false,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat>
            {
                new MatchPlayerStat { AccountId = targetAccountId, Kills = 5 },
                new MatchPlayerStat { AccountId = otherAccountId, Kills = 2 } 
            }
        };

        var match3 = new Match
        {
            Id = 3333333L,
            Duration = 1500,
            RadiantWin = true,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat>
            {
                new MatchPlayerStat { AccountId = otherAccountId, Kills = 15 }
            }
        };

        _inMemoryDbContext.Matches.AddRange(match1, match2, match3);
        await _inMemoryDbContext.SaveChangesAsync();

        // Act
        long? accountIdToFilter = targetAccountId;

        IQueryable<Match> query = _inMemoryDbContext.Matches;

        if (accountIdToFilter.HasValue)
        {
            query = query.Where(m => m.Players.Any(p => p.AccountId == accountIdToFilter.Value));
        }

        var result = await query.Include(m => m.Players).ToListAsync();

        // Assert
        result.Should().HaveCount(2, "Match 1 and match 2 need to return we have target player");

        result.All(m => m.Players.Any(p => p.AccountId == targetAccountId))
              .Should().BeTrue("All matches need to include of target player");

        result.Any(m => m.Id == 3333333L).Should().BeFalse("Match without target player needn't be in results");
    }
}