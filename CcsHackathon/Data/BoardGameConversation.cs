namespace CcsHackathon.Data;

public class BoardGameConversation
{
    public Guid Id { get; set; }
    public Guid BoardGameId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    // Navigation properties
    public BoardGame BoardGame { get; set; } = null!;
    public ICollection<BoardGameConversationMessage> Messages { get; set; } = new List<BoardGameConversationMessage>();
}

