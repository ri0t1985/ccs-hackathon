using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class BoardGameAiBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BoardGameAiBackgroundService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromMinutes(5); // Process every 5 minutes
    private readonly int _maxRetries = 3;
    private readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(30);

    public BoardGameAiBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BoardGameAiBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Board Game AI Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessGamesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Board Game AI Background Service execution loop");
            }

            // Wait before next processing cycle
            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Board Game AI Background Service stopped");
    }

    private async Task ProcessGamesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var aiService = scope.ServiceProvider.GetRequiredService<IBoardGameAiService>();

        try
        {
            // Find games that need AI data
            // Look for GameRegistrations where the game name doesn't have AI data yet
            var gamesNeedingData = await dbContext.GameRegistrations
                .Include(gr => gr.BoardGameCache)
                .Where(gr => 
                    gr.BoardGameCache == null || 
                    !gr.BoardGameCache.HasAiData ||
                    string.IsNullOrWhiteSpace(gr.BoardGameCache.Summary))
                .Select(gr => new
                {
                    GameRegistrationId = gr.Id,
                    GameName = gr.GameId,
                    BoardGameCache = gr.BoardGameCache
                })
                .ToListAsync(cancellationToken);

            if (!gamesNeedingData.Any())
            {
                _logger.LogDebug("No games need AI data processing");
                return;
            }

            _logger.LogInformation("Found {Count} games needing AI data", gamesNeedingData.Count);

            foreach (var game in gamesNeedingData)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ProcessGameAsync(dbContext, aiService, game.GameRegistrationId, game.GameName, game.BoardGameCache, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing games for AI data");
        }
    }

    private async Task ProcessGameAsync(
        ApplicationDbContext dbContext,
        IBoardGameAiService aiService,
        Guid gameRegistrationId,
        string gameName,
        BoardGameCache? existingCache,
        CancellationToken cancellationToken)
    {
        // Idempotent check: if AI data already exists and is complete, skip
        if (existingCache != null && existingCache.HasAiData && !string.IsNullOrWhiteSpace(existingCache.Summary))
        {
            _logger.LogDebug("Game {GameName} already has AI data, skipping", gameName);
            return;
        }

        _logger.LogInformation("Processing AI data for game: {GameName}", gameName);

        int attempt = 0;
        BoardGameAiData? aiData = null;

        // Retry logic
        while (attempt < _maxRetries && aiData == null)
        {
            attempt++;
            try
            {
                aiData = await aiService.GenerateAiDataAsync(gameName);
                
                if (aiData != null)
                {
                    break; // Success
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Attempt {Attempt} failed to generate AI data for game {GameName}", attempt, gameName);
                
                if (attempt < _maxRetries)
                {
                    await Task.Delay(_retryDelay, cancellationToken);
                }
            }
        }

        if (aiData == null)
        {
            _logger.LogError("Failed to generate AI data for game {GameName} after {Attempts} attempts", gameName, _maxRetries);
            return;
        }

        try
        {
            // Update or create BoardGameCache
            if (existingCache == null)
            {
                // Check if a cache exists by game name (unique constraint)
                var existingByName = await dbContext.BoardGameCaches
                    .FirstOrDefaultAsync(c => c.GameName == gameName, cancellationToken);

                if (existingByName != null)
                {
                    // Update existing cache
                    existingByName.Complexity = aiData.Complexity;
                    existingByName.TimeToSetupMinutes = aiData.TimeToSetupMinutes;
                    existingByName.Summary = aiData.Summary;
                    existingByName.HasAiData = true;
                    existingByName.LastUpdatedAt = DateTime.UtcNow;
                    
                    // Update the GameRegistration to point to this cache
                    var gameReg = await dbContext.GameRegistrations
                        .FirstOrDefaultAsync(gr => gr.Id == gameRegistrationId, cancellationToken);
                    if (gameReg != null)
                    {
                        gameReg.BoardGameCache = existingByName;
                    }
                }
                else
                {
                    // Create new cache
                    var newCache = new BoardGameCache
                    {
                        Id = Guid.NewGuid(),
                        GameRegistrationId = gameRegistrationId,
                        GameName = gameName,
                        Complexity = aiData.Complexity,
                        TimeToSetupMinutes = aiData.TimeToSetupMinutes,
                        Summary = aiData.Summary,
                        HasAiData = true,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    dbContext.BoardGameCaches.Add(newCache);
                }
            }
            else
            {
                // Update existing cache
                existingCache.Complexity = aiData.Complexity;
                existingCache.TimeToSetupMinutes = aiData.TimeToSetupMinutes;
                existingCache.Summary = aiData.Summary;
                existingCache.HasAiData = true;
                existingCache.LastUpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Successfully saved AI data for game: {GameName}", gameName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving AI data for game: {GameName}", gameName);
        }
    }
}

