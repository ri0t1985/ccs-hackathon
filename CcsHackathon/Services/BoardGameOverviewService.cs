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

    public async Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null)
    {
        // Require sessionId - only return games for a specific session
        if (!sessionId.HasValue)
        {
            return Enumerable.Empty<BoardGameOverviewItem>();
        }

        // Get board games from GameRegistrations that are registered for the specified session
        IQueryable<GameRegistration> gameRegistrationsQuery = _dbContext.GameRegistrations
            .Include(gr => gr.Registration)
            .Include(gr => gr.BoardGame)
            .Include(gr => gr.BoardGameCache)
            .Where(gr => gr.Registration.SessionId == sessionId.Value);

        var gameRegistrations = await gameRegistrationsQuery.ToListAsync();

        // Group by BoardGameId to get unique games, preferring entries with AI data
        var uniqueGames = gameRegistrations
            .GroupBy(gr => gr.BoardGameId)
            .Select(g => g.OrderByDescending(gr => gr.BoardGameCache?.HasAiData ?? false).First())
            .ToList();

        // Build result
        var games = uniqueGames
            .Select(gr => new BoardGameOverviewItem
            {
                GameName = gr.BoardGame.Name,
                Summary = gr.BoardGameCache?.Summary,
                Complexity = gr.BoardGameCache?.Complexity,
                TimeToSetupMinutes = gr.BoardGameCache?.TimeToSetupMinutes,
                HasAiData = gr.BoardGameCache?.HasAiData ?? false,
                LastUpdatedAt = gr.BoardGameCache?.LastUpdatedAt ?? gr.Registration.CreatedAt
            })
            .OrderBy(g => g.GameName)
            .ToList();

        return games;
    }
}

