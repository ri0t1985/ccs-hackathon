using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class BoardGameOverviewService : IBoardGameOverviewService
{
    private readonly ApplicationDbContext _dbContext;

    public BoardGameOverviewService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null, string? userId = null)
    {
        // Get all unique game names from GameRegistrations (including those without AI data)
        IQueryable<GameRegistration> gameRegistrationsQuery = _dbContext.GameRegistrations
            .Include(gr => gr.Registration)
            .Include(gr => gr.BoardGameCache);

        // If sessionId is provided, filter games that are registered for that session
        if (sessionId.HasValue)
        {
            gameRegistrationsQuery = gameRegistrationsQuery.Where(gr => gr.Registration.SessionId == sessionId.Value);
        }

        var gameRegistrations = await gameRegistrationsQuery.ToListAsync();

        // Group by game name to get unique games, preferring entries with AI data
        var uniqueGames = gameRegistrations
            .GroupBy(gr => gr.GameId)
            .Select(g => g.OrderByDescending(gr => gr.BoardGameCache?.HasAiData ?? false).First())
            .ToList();

        // Get all game names for participant queries
        var gameNames = uniqueGames.Select(gr => gr.GameId).ToList();

        // Fetch participant data in a single query if sessionId is provided
        Dictionary<string, int> participantCounts = new();
        HashSet<string> userParticipatingGames = new();

        if (sessionId.HasValue && gameNames.Any())
        {
            // Get participant counts for all games in one query
            var participantCountsQuery = await _dbContext.GameParticipants
                .Where(gp => gp.SessionId == sessionId.Value && gameNames.Contains(gp.GameName))
                .GroupBy(gp => gp.GameName)
                .Select(g => new { GameName = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var pc in participantCountsQuery)
            {
                participantCounts[pc.GameName] = pc.Count;
            }

            // Get user participation status for all games in one query
            if (!string.IsNullOrEmpty(userId))
            {
                var userParticipations = await _dbContext.GameParticipants
                    .Where(gp => gp.SessionId == sessionId.Value 
                        && gp.UserId == userId 
                        && gameNames.Contains(gp.GameName))
                    .Select(gp => gp.GameName)
                    .ToListAsync();

                userParticipatingGames = userParticipations.ToHashSet();
            }
        }

        // Build result with all data
        var games = uniqueGames
            .Select(gr => new BoardGameOverviewItem
            {
                GameName = gr.GameId,
                Summary = gr.BoardGameCache?.Summary,
                Complexity = gr.BoardGameCache?.Complexity,
                TimeToSetupMinutes = gr.BoardGameCache?.TimeToSetupMinutes,
                HasAiData = gr.BoardGameCache?.HasAiData ?? false,
                LastUpdatedAt = gr.BoardGameCache?.LastUpdatedAt ?? gr.Registration.CreatedAt,
                ParticipantCount = participantCounts.GetValueOrDefault(gr.GameId, 0),
                IsUserParticipating = userParticipatingGames.Contains(gr.GameId)
            })
            .OrderBy(g => g.GameName)
            .ToList();

        return games;
    }
}

