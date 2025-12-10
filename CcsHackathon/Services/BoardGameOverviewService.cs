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
        IQueryable<BoardGameCache> query = _dbContext.BoardGameCaches
            .Include(g => g.GameRegistration)
                .ThenInclude(gr => gr.Registration);

        // If sessionId is provided, filter games that are registered for that session
        if (sessionId.HasValue)
        {
            query = query.Where(g => g.GameRegistration.Registration.SessionId == sessionId.Value);
        }

        // Get all games first, then project
        var gamesList = await query.ToListAsync();

        var games = gamesList
            .OrderBy(g => g.GameName)
            .Select(g => new BoardGameOverviewItem
            {
                GameName = g.GameName,
                Summary = g.Summary,
                Complexity = g.Complexity,
                TimeToSetupMinutes = g.TimeToSetupMinutes,
                HasAiData = g.HasAiData,
                LastUpdatedAt = g.LastUpdatedAt
            })
            .ToList();

        return games;
    }
}

