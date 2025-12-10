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

    public async Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync()
    {
        var games = await _dbContext.BoardGameCaches
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
            .ToListAsync();

        return games;
    }
}

