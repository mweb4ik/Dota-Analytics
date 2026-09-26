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
// ЯДЕРНЫЙ ЭНДПОИНТ ДЛЯ ОБНОВЛЕНИЯ ДАННЫХ (удалить перед финальной сдачей)
// ========================================================================
app.MapPost("/api/refresh", async (AppDbContext context, MatchCacheService cacheService) =>
{
    try
    {
        Console.WriteLine("[REFRESH] Начало очистки базы данных...");

        // 1. Сначала удаляем статистику игроков (чтобы не нарушить внешние ключи)
        context.MatchPlayerStats.RemoveRange(context.MatchPlayerStats);

        // 2. Удаляем сами матчи
        context.Matches.RemoveRange(context.Matches);

        // 3. Сохраняем удаление в БД
        await context.SaveChangesAsync();
        Console.WriteLine("[REFRESH] База данных очищена.");

        // 4. Загружаем свежие про-матчи из OpenDota
        Console.WriteLine("[REFRESH] Начало загрузки свежих данных из OpenDota...");
        await cacheService.PreloadProMatchesAsync();
        Console.WriteLine("[REFRESH] Данные успешно обновлены!");

        return Results.Ok(new
        {
            message = "База очищена и успешно обновлена свежими про-матчами",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[REFRESH ERROR] {ex.Message}");
        return Results.Problem(
            detail: $"Ошибка при обновлении данных: {ex.Message}",
            statusCode: 500
        );
    }
});
// ========================================================================

await app.RunAsync();