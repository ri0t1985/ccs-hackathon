using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameService
{
    Task<IEnumerable<BoardGame>> SearchBoardGamesAsync(string searchTerm, int maxResults = 10);
    Task<IEnumerable<BoardGame>> GetRecentBoardGamesAsync(int count = 5);
    Task<BoardGame?> GetBoardGameByIdAsync(Guid id);
    Task<BoardGame> CreateBoardGameAsync(string name);
    Task<bool> BoardGameExistsAsync(string name);
}

