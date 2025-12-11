namespace CcsHackathon.Data;

public class BoardGame
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? SetupComplexity { get; set; } // Using decimal to match Complexity in BoardGameCache
    public decimal? Score { get; set; } // AI score/rating
    public int? AveragePlaytimeMinutes { get; set; } // Average play time in minutes
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<GameRegistration> GameRegistrations { get; set; } = new List<GameRegistration>();
    public ICollection<BoardGameCache> BoardGameCaches { get; set; } = new List<BoardGameCache>();
    public BoardGameMetadata? Metadata { get; set; }
}

