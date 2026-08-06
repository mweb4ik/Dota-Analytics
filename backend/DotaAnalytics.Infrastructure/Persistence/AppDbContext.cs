
using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Modules.Matches.Models;
using DotaAnalytics.Modules.Matches.Data;
namespace DotaAnalytics.Infrastructure.Persistence;
public class AppDbContext(DbContextOptions <AppDbContext> options) : DbContext(options) 
{
public DbSet<Match> Matches {get;set;}
public DbSet<MatchPlayerStat> MatchPlayerStats {get;set;}
protected override void OnModelCreating(ModelBuilder modelBuilder){
    modelBuilder.ApplyConfiguration(new MatchConfiguration());
    modelBuilder.ApplyConfiguration(new MatchPlayerStatConfiguration());
    base.OnModelCreating(modelBuilder);
}
}