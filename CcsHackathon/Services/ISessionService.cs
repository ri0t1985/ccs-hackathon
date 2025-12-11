using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface ISessionService
{
    Task<Session> CreateSessionAsync(DateOnly date);
    Task<IEnumerable<Session>> GetAllSessionsAsync();
    Task<IEnumerable<Session>> GetUpcomingSessionsAsync();
    Task<IEnumerable<Session>> GetHistoricSessionsAsync();
    Task<IEnumerable<SessionWithAttendeeCount>> GetUpcomingSessionsWithAttendeeCountAsync();
    Task<IEnumerable<SessionWithAttendeeCount>> GetHistoricSessionsWithAttendeeCountAsync();
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<Session> UpdateSessionDateAsync(Guid sessionId, DateOnly newDate);
    Task CancelSessionAsync(Guid sessionId);
}

public class SessionWithAttendeeCount
{
    public Session Session { get; set; } = null!;
    public int AttendeeCount { get; set; }
}

