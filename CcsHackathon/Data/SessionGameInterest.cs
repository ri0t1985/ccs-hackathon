namespace CcsHackathon.Data;

public class SessionGameInterest
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid BoardGameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public Session Session { get; set; } = null!;
    public BoardGame BoardGame { get; set; } = null!;
}

