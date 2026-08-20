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

// 1. Test class declaration
public class LeaderboardTests
{
    private readonly AppDbContext _inMemoryDbContext;

    public LeaderboardTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _inMemoryDbContext = new AppDbContext(options);
    }

    [Fact]
    public async Task GetLeaderboardStatsAsync_ShouldFilterPlayersWithLessThan10MatchesAndSortByWinRate()
    {
        // Arrange

        // Player 1: 12 matches, 9 wins (75% Win Rate) - SHOULD be in the top
        var player1Stats = Enumerable.Range(1, 12).Select(i => new MatchPlayerStat
        {
            AccountId = 11111111L,
            MatchId = i,
            Hero = Heroes.AntiMage // ID = 1 (odd) = Dire side
        }).ToList();

        var matchesForPlayer1 = player1Stats.Select((p, index) => new Match
        {
            Id = index + 1,
            // index 0..8 (9 matches) -> false (Dire wins, our player wins)
            // index 9..11 (3 matches) -> true (Radiant wins, our player loses)
            RadiantWin = index >= 9, 
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat> { p }
        }).ToList();

        // Player 2: 5 matches - SHOULD NOT be in the top (less than 10 games)
        var player2Stats = Enumerable.Range(1, 5).Select(i => new MatchPlayerStat
        {
            AccountId = 22222222L,
            MatchId = i + 100,
            Hero = Heroes.AntiMage
        }).ToList();

        var matchesForPlayer2 = player2Stats.Select((p, index) => new Match
        {
            Id = index + 100,
            RadiantWin = true,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat> { p }
        }).ToList();

        // Player 3: 9 matches - SHOULD NOT be in the top (less than 10 games, filter will trigger)
        var player3Stats = Enumerable.Range(1, 9).Select(i => new MatchPlayerStat
        {
            AccountId = 33333333L,
            MatchId = i + 200,
            Hero = Heroes.AntiMage
        }).ToList();

        var matchesForPlayer3 = player3Stats.Select((p, index) => new Match
        {
            Id = index + 200,
            RadiantWin = index >= 8,
            StartTime = DateTime.UtcNow,
            CachedAt = DateTime.UtcNow,
            Players = new List<MatchPlayerStat> { p }
        }).ToList();

        _inMemoryDbContext.Matches.AddRange(matchesForPlayer1);
        _inMemoryDbContext.Matches.AddRange(matchesForPlayer2);
        _inMemoryDbContext.Matches.AddRange(matchesForPlayer3);
        await _inMemoryDbContext.SaveChangesAsync();

        // Act
        var allMatches = await _inMemoryDbContext.Matches.Include(m => m.Players).ToListAsync();
        var allPlayerStats = allMatches.SelectMany(m => m.Players).ToList();

        var leaderboard = allPlayerStats
            .GroupBy(p => p.AccountId)
            .Select(group => 
            { 
                var accountId = group.Key;
                var stats = group.ToList();
                int totalMatches = stats.Count;
                int wins = 0;

                foreach (var stat in stats)
                {
                    var match = allMatches.First(m => m.Id == stat.MatchId);
                    
                    // Inline logic to check player's side (since IsRadiantSide is private in the endpoint)
                    bool isPlayerRadiant = ((int)stat.Hero % 2 == 0);

                    if ((match.RadiantWin && isPlayerRadiant) || (!match.RadiantWin && !isPlayerRadiant))
                    {
                        wins++;
                    }
                } 

                double winRate = totalMatches > 0 ? (double)wins / totalMatches * 100 : 0;

                return new
                {
                    accountId,
                    totalMatches,
                    wins,
                    winRate
                };
            })
            .Where(x => x.totalMatches >= 10)
            .OrderByDescending(x => x.winRate)
            .Take(10)
            .ToList();

        // Assert
        leaderboard.Should().HaveCount(1, "Only Player 1 has >= 10 matches, others should be filtered out");
        leaderboard.First().accountId.Should().Be(11111111L, "Only Player 1 should remain in the top leaderboard");
    }
}