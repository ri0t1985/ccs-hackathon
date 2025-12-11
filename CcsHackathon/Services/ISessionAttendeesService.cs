using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface ISessionAttendeesService
{
    Task<SessionAttendeesInfo?> GetSessionAttendeesAsync(Guid sessionId);
}

public class SessionAttendeesInfo
{
    public Session Session { get; set; } = null!;
    public List<AttendeeInfo> Attendees { get; set; } = new();
}

public class AttendeeInfo
{
    public string UserDisplayName { get; set; } = string.Empty;
    public List<string> BoardGames { get; set; } = new();
    public string? FoodRequirements { get; set; }
}

