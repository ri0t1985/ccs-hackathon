namespace CcsHackathon.Data;

public class GameRating
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid BoardGameId { get; set; }
    public Guid SessionId { get; set; }
    public int Rating { get; set; } // 0-5 stars
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public BoardGame BoardGame { get; set; } = null!;
    public Session Session { get; set; } = null!;
}

