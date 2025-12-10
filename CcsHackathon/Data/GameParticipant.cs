namespace CcsHackathon.Data;

public class GameParticipant
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation property for many-to-one relationship with Session
    public Session Session { get; set; } = null!;
}

