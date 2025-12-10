using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(DateTime date);
    Task<IEnumerable<Session>> GetAllSessionsAsync();
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<Session> UpdateSessionDateAsync(Guid sessionId, DateTime newDate);
    Task CancelSessionAsync(Guid sessionId);
    Task<int> GetParticipantCountAsync(Guid sessionId, string gameName);
    Task<bool> AddParticipantAsync(Guid sessionId, string gameName, string userId, string userDisplayName);
    Task<bool> RemoveParticipantAsync(Guid sessionId, string gameName, string userId);
    Task<bool> IsParticipantAsync(Guid sessionId, string gameName, string userId);
}

