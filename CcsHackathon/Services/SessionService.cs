using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class SessionService : ISessionService
{
    private readonly ApplicationDbContext _dbContext;

    public SessionService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Session> CreateSessionAsync(DateTime date)
    {
        var session = new Session
        {
            Id = Guid.NewGuid(),
            Date = date.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            IsCancelled = false
        };

        _dbContext.Sessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<IEnumerable<Session>> GetAllSessionsAsync()
    {
        return await _dbContext.Sessions
            .Where(s => !s.IsCancelled)
            .OrderBy(s => s.Date)
            .ToListAsync();
    }

    public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
    {
        return await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task<Session> UpdateSessionDateAsync(Guid sessionId, DateTime newDate)
    {
        var session = await _dbContext.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} not found.");
        }

        session.Date = newDate.ToUniversalTime();
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task CancelSessionAsync(Guid sessionId)
    {
        var session = await _dbContext.Sessions.FindAsync(sessionId);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {sessionId} not found.");
        }

        session.IsCancelled = true;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetParticipantCountAsync(Guid sessionId, string gameName)
    {
        return await _dbContext.GameParticipants
            .CountAsync(p => p.SessionId == sessionId && p.GameName == gameName);
    }

    public async Task<bool> AddParticipantAsync(Guid sessionId, string gameName, string userId, string userDisplayName)
    {
        // Check if already a participant
        var existing = await _dbContext.GameParticipants
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.GameName == gameName && p.UserId == userId);

        if (existing != null)
        {
            return false; // Already a participant
        }

        var participant = new GameParticipant
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            GameName = gameName,
            UserId = userId,
            UserDisplayName = userDisplayName,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.GameParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveParticipantAsync(Guid sessionId, string gameName, string userId)
    {
        var participant = await _dbContext.GameParticipants
            .FirstOrDefaultAsync(p => p.SessionId == sessionId && p.GameName == gameName && p.UserId == userId);

        if (participant == null)
        {
            return false;
        }

        _dbContext.GameParticipants.Remove(participant);
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsParticipantAsync(Guid sessionId, string gameName, string userId)
    {
        return await _dbContext.GameParticipants
            .AnyAsync(p => p.SessionId == sessionId && p.GameName == gameName && p.UserId == userId);
    }
}

