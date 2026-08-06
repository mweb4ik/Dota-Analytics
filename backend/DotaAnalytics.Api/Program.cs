using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Api.Services;       
using DotaAnalytics.Api.Middleware;      

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<MatchProcessingService>();
builder.Services.AddScoped<MatchCacheService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate(); 
}

app.Lifetime.ApplicationStarted.Register(async () =>
{
    using var scope = app.Services.CreateScope();
    var cacheService = scope.ServiceProvider.GetRequiredService<MatchCacheService>();
    await cacheService.PreloadProMatchesAsync();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.MapGet("/", () => "Dota Analytics API is running!");

DotaAnalytics.Api.Endpoints.FetchMatchEndpoint.Map(app);
DotaAnalytics.Api.Endpoints.MatchesEndpoints.Map(app);
DotaAnalytics.Api.Endpoints.MatchPlayerStatsEndpoints.Map(app);

app.Run();