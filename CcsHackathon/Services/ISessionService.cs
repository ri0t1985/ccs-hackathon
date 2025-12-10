using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(DateOnly date);
    Task<IEnumerable<Session>> GetAllSessionsAsync();
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<Session> UpdateSessionDateAsync(Guid sessionId, DateOnly newDate);
    Task CancelSessionAsync(Guid sessionId);
}

