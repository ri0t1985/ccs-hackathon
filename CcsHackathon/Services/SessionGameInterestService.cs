using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class SessionGameInterestService : ISessionGameInterestService
{
    private readonly ApplicationDbContext _dbContext;

    public SessionGameInterestService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AddInterestAsync(string userId, Guid sessionId, Guid boardGameId)
    {
        // Check if interest already exists
        var existing = await _dbContext.SessionGameInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.SessionId == sessionId && i.BoardGameId == boardGameId);

        if (existing != null)
        {
            return false; // Already exists
        }

        // Verify session is upcoming (not historical)
        var session = await _dbContext.Sessions.FindAsync(sessionId);
        if (session == null || session.IsCancelled)
        {
            throw new ArgumentException("Session not found or is cancelled.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (session.Date < today)
        {
            throw new InvalidOperationException("Cannot add interest to historical sessions.");
        }

        // Verify board game exists
        var boardGame = await _dbContext.BoardGames.FindAsync(boardGameId);
        if (boardGame == null)
        {
            throw new ArgumentException("Board game not found.");
        }

        var interest = new SessionGameInterest
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SessionId = sessionId,
            BoardGameId = boardGameId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.SessionGameInterests.Add(interest);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveInterestAsync(string userId, Guid sessionId, Guid boardGameId)
    {
        var interest = await _dbContext.SessionGameInterests
            .FirstOrDefaultAsync(i => i.UserId == userId && i.SessionId == sessionId && i.BoardGameId == boardGameId);

        if (interest == null)
        {
            return false; // Doesn't exist
        }

        _dbContext.SessionGameInterests.Remove(interest);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> HasInterestAsync(string userId, Guid sessionId, Guid boardGameId)
    {
        return await _dbContext.SessionGameInterests
            .AnyAsync(i => i.UserId == userId && i.SessionId == sessionId && i.BoardGameId == boardGameId);
    }

    public async Task<Dictionary<Guid, int>> GetInterestCountsAsync(Guid sessionId, List<Guid> boardGameIds)
    {
        if (!boardGameIds.Any())
        {
            return new Dictionary<Guid, int>();
        }

        var counts = await _dbContext.SessionGameInterests
            .Where(i => i.SessionId == sessionId && boardGameIds.Contains(i.BoardGameId))
            .GroupBy(i => i.BoardGameId)
            .Select(g => new { BoardGameId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BoardGameId, x => x.Count);

        // Ensure all board game IDs are in the dictionary (with count 0 if no interests)
        var result = new Dictionary<Guid, int>();
        foreach (var boardGameId in boardGameIds)
        {
            result[boardGameId] = counts.GetValueOrDefault(boardGameId, 0);
        }

        return result;
    }

    public async Task<HashSet<Guid>> GetUserInterestsAsync(string userId, Guid sessionId, List<Guid> boardGameIds)
    {
        if (!boardGameIds.Any())
        {
            return new HashSet<Guid>();
        }

        var interests = await _dbContext.SessionGameInterests
            .Where(i => i.UserId == userId && i.SessionId == sessionId && boardGameIds.Contains(i.BoardGameId))
            .Select(i => i.BoardGameId)
            .ToListAsync();

        return interests.ToHashSet();
    }
}

