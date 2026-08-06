using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DotaAnalytics.Modules.Matches.Models;
namespace DotaAnalytics.Modules.Matches.Data;
public class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder){
    builder.ToTable("matches");
    builder.HasKey(m => m.Id);
}
}
