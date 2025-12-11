using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class BoardGameService : IBoardGameService
{
    private readonly ApplicationDbContext _dbContext;

    public BoardGameService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<BoardGame>> SearchBoardGamesAsync(string searchTerm, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return Enumerable.Empty<BoardGame>();
        }

        var normalizedSearch = searchTerm.Trim();
        
        return await _dbContext.BoardGames
            .Where(bg => bg.Name.ToLower().Contains(normalizedSearch.ToLower()))
            .OrderBy(bg => bg.Name)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<IEnumerable<BoardGame>> GetRecentBoardGamesAsync(int count = 5)
    {
        return await _dbContext.BoardGames
            .OrderByDescending(bg => bg.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<BoardGame?> GetBoardGameByIdAsync(Guid id)
    {
        return await _dbContext.BoardGames
            .FirstOrDefaultAsync(bg => bg.Id == id);
    }

    public async Task<BoardGame> CreateBoardGameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Board game name cannot be empty.", nameof(name));
        }

        var normalizedName = name.Trim();
        
        // Check for duplicates (case-insensitive)
        var existing = await _dbContext.BoardGames
            .FirstOrDefaultAsync(bg => bg.Name.ToLower() == normalizedName.ToLower());
        
        if (existing != null)
        {
            throw new InvalidOperationException($"A board game with the name '{normalizedName}' already exists.");
        }

        var boardGame = new BoardGame
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.BoardGames.Add(boardGame);
        await _dbContext.SaveChangesAsync();

        return boardGame;
    }

    public async Task<bool> BoardGameExistsAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var normalizedName = name.Trim();
        
        return await _dbContext.BoardGames
            .AnyAsync(bg => bg.Name.ToLower() == normalizedName.ToLower());
    }
}

