using Microsoft.EntityFrameworkCore;
using DotaAnalytics.Infrastructure.Persistence;
using DotaAnalytics.Api.Services;
using DotaAnalytics.Api.Middleware;

Environment.SetEnvironmentVariable("DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE", "false");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
builder.Services.AddHttpClient();

builder.Services.AddScoped<MatchProcessingService>();
builder.Services.AddScoped<MatchCacheService>();
builder.Services.AddScoped<OpenDotaService>();

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

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.MapGet("/", () => "Dota Analytics API is running!");

DotaAnalytics.Api.Endpoints.FetchMatchEndpoint.Map(app);
DotaAnalytics.Api.Endpoints.MatchesEndpoints.Map(app);
DotaAnalytics.Api.Endpoints.MatchPlayerStatsEndpoints.Map(app);
DotaAnalytics.Api.Endpoints.PlayerStatsEndpoints.Map(app);

// ============================================
// ВРЕМЕННЫЕ ЭНДПОИНТЫ ДЛЯ ОБНОВЛЕНИЯ БАЗЫ НА RENDER
// ============================================

// 1. Очистка базы данных
app.MapDelete("/api/admin/clear-database", async (AppDbContext context) =>
{
    context.MatchPlayerStats.RemoveRange(context.MatchPlayerStats);
    context.Matches.RemoveRange(context.Matches);
    await context.SaveChangesAsync();
    return Results.Ok("База данных полностью очищена!");
});

// 2. Массовая загрузка свежих матчей
app.MapPost("/api/admin/load-recent-matches", async (
    AppDbContext context,
    HttpClient httpClient,
    OpenDotaService openDotaService,
    int count = 20) =>
{
    var response = await httpClient.GetAsync("https://api.opendota.com/api/proMatches");

    if (!response.IsSuccessStatusCode)
        return Results.BadRequest($"Не удалось получить список матчей. Статус: {response.StatusCode}");

    var json = await response.Content.ReadAsStringAsync();
    var matches = System.Text.Json.JsonSerializer.Deserialize<List<ProMatch>>(json, new System.Text.Json.JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (matches == null || !matches.Any())
        return Results.BadRequest("Список матчей пуст");

    var matchesToLoad = matches.Take(count).ToList();

    int loaded = 0;
    int skipped = 0;
    int errors = 0;
    var errorDetails = new List<string>();

    foreach (var match in matchesToLoad)
    {
        try
        {
            var result = await openDotaService.FetchAndSaveMatchAsync(match.MatchId);

            if (result)
                loaded++;
            else
                skipped++;

            // Задержка чтобы не получить бан от OpenDota
            await Task.Delay(1500);
        }
        catch (Exception ex)
        {
            errors++;
            errorDetails.Add($"Матч {match.MatchId}: {ex.Message}");
        }
    }

    return Results.Ok(new
    {
        message = "Массовая загрузка завершена",
        loaded,
        skipped,
        errors,
        total = matchesToLoad.Count,
        errorDetails
    });
});

await app.RunAsync();

// Временный класс для парсинга списка про-матчей
public class ProMatch
{
    [System.Text.Json.Serialization.JsonPropertyName("match_id")]
    public long MatchId { get; set; }
}