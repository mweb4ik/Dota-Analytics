using DotaAnalytics.Api.Middleware;
using DotaAnalytics.Api.Services;
using DotaAnalytics.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:3000",
            "http://127.0.0.1:5173",
            "http://127.0.0.1:3000"
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

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
app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();

app.MapGet("/", () => "Dota Analytics API is running!");

DotaAnalytics.Api.Endpoints.FetchMatchEndpoint.Map(app);
DotaAnalytics.Api.Endpoints.MatchesEndpoints.Map(app);
DotaAnalytics.Api.Endpoints.MatchPlayerStatsEndpoints.Map(app);
DotaAnalytics.Api.Endpoints.PlayerStatsEndpoints.Map(app);

// ========================================================================
// ЭНДПОИНТЫ ДЛЯ УПРАВЛЕНИЯ ДАННЫМИ (удалить потом)
// ========================================================================

// 1. ЭНДПОИНТ ДЛЯ ОЧИСТКИ (Работает мгновенно)
app.MapPost("/api/refresh/clear", async (AppDbContext context, IMemoryCache memoryCache) =>
{
    try
    {
        Console.WriteLine("[CLEAR] Начало очистки базы данных...");
        context.MatchPlayerStats.RemoveRange(context.MatchPlayerStats);
        context.Matches.RemoveRange(context.Matches);
        await context.SaveChangesAsync();

        if (memoryCache is MemoryCache concreteCache)
        {
            concreteCache.Compact(1.0);
        }

        Console.WriteLine("[CLEAR] База данных и кэш успешно очищены.");
        return Results.Ok(new { message = "База и кэш очищены" });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: $"Ошибка очистки: {ex.Message}", statusCode: 500);
    }
});

// 2. ЭНДПОИНТ ДЛЯ ЗАГРУЗКИ 
app.MapPost("/api/refresh/fetch", async (MatchCacheService cacheService) =>
{
    try
    {
        Console.WriteLine("[FETCH] Начало загрузки свежих данных из OpenDota...");

        await cacheService.PreloadProMatchesAsync();

        Console.WriteLine("[FETCH] Данные успешно загружены и сохранены в БД!");
        return Results.Ok(new { message = "Данные успешно загружены из OpenDota" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FETCH ERROR] {ex.Message}");
        return Results.Problem(detail: $"Ошибка загрузки: {ex.Message}", statusCode: 500);
    }
});
// ========================================================================
await app.RunAsync();