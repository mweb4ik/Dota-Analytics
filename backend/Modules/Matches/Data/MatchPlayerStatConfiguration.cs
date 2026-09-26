using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DotaAnalytics.Modules.Matches.Models;

namespace DotaAnalytics.Modules.Matches.Data;

public class MatchPlayerStatConfiguration : IEntityTypeConfiguration<MatchPlayerStat>
{
    public void Configure(EntityTypeBuilder<MatchPlayerStat> builder)
    {
        builder.ToTable("match_player_stats");
        builder.HasKey(m => m.Id);

        builder.HasIndex(m => m.MatchId);
        builder.HasIndex(m => m.AccountId);

        
        builder.HasOne(p => p.Match)
               .WithMany(m => m.Players)
               .HasForeignKey(p => p.MatchId)
               .OnDelete(DeleteBehavior.Cascade); 
    }
}