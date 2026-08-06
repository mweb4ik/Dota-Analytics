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

// Отладка ДО регистрации DbContext
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"[DEBUG] ConnectionString length: {connStr?.Length ?? 0}");
if (!string.IsNullOrEmpty(connStr))
{
    Console.WriteLine($"[DEBUG] Starts with: {connStr.Substring(0, Math.Min(30, connStr.Length))}");
}
else
{
    Console.WriteLine("[ERROR] ConnectionString is NULL or EMPTY!");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddMemoryCache();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync(); 
}

app.Lifetime.ApplicationStarted.Register(async () =>
{
    await using var scope = app.Services.CreateAsyncScope();
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

await app.RunAsync();