using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameAiService
{
    Task<BoardGameAiData?> GenerateAiDataAsync(string gameName);
}

public class BoardGameAiData
{
    public decimal Complexity { get; set; }
    public int TimeToSetupMinutes { get; set; }
    public int AveragePlaytimeMinutes { get; set; }
    public string Summary { get; set; } = string.Empty;
    
    // Metadata fields
    public string? GameType { get; set; }
    public string? Theme { get; set; }
    public string? PlayerInteractionLevel { get; set; }
    public string? SkillRequirements { get; set; }
    public string? RandomnessLevel { get; set; }
    public string? ComplexityTier { get; set; }
    public string? TargetAudience { get; set; }
    public int? ReplayabilityScore { get; set; }
    public string? LearningCurve { get; set; }
    public string? TypicalPlayStyle { get; set; }
}

