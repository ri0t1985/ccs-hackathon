namespace CcsHackathon.Data;

public class BoardGameCache
{
    public Guid Id { get; set; }
    public Guid GameRegistrationId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public string? RulesText { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    
    // AI-generated fields
    public decimal? Complexity { get; set; }
    public int? TimeToSetupMinutes { get; set; }
    public string? Summary { get; set; }
    public bool HasAiData { get; set; }
    
    // Navigation property for one-to-one relationship with GameRegistration
    public GameRegistration GameRegistration { get; set; } = null!;
}

