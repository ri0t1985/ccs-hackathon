namespace CcsHackathon.Data;

public class BoardGameFaqCache
{
    public Guid Id { get; set; }
    public Guid BoardGameId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    // Navigation property
    public BoardGame BoardGame { get; set; } = null!;
}

