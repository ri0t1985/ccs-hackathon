namespace CcsHackathon.Data;

public class BoardGameMetadata
{
    public Guid Id { get; set; }
    public Guid BoardGameId { get; set; }
    
    // Game Type / Genre
    public string? GameType { get; set; } // Strategy, Party, Cooperative, etc.
    
    // Theme
    public string? Theme { get; set; } // Fantasy, Sci-fi, Horror, etc.
    
    // Player Interaction Level
    public string? PlayerInteractionLevel { get; set; } // Low, Medium, High
    
    // Skill Requirements (comma-separated or JSON)
    public string? SkillRequirements { get; set; } // Planning, Negotiation, Bluffing, etc.
    
    // Randomness Level
    public string? RandomnessLevel { get; set; } // Low, Medium, High
    
    // Complexity Tier
    public string? ComplexityTier { get; set; } // Light, Medium, Heavy
    
    // Target Audience
    public string? TargetAudience { get; set; } // Casual players, Families, Hardcore gamers
    
    // Replayability Score (1-10 or similar)
    public int? ReplayabilityScore { get; set; }
    
    // Learning Curve
    public string? LearningCurve { get; set; } // Easy, Moderate, Steep
    
    // Typical Play Style
    public string? TypicalPlayStyle { get; set; } // Competitive, Cooperative, Team-based, Solo-friendly
    
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    // Navigation property
    public BoardGame BoardGame { get; set; } = null!;
}

