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

        // Build result
        var games = uniqueGames
            .Select(gr => new BoardGameOverviewItem
            {
                GameName = gr.GameId,
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

