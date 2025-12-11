namespace CcsHackathon.Services;

public interface IGameRecommendationService
{
    Task<GameRecommendationResult> GetRecommendationsAsync(string userId);
}

public class GameRecommendationResult
{
    public bool IsFallbackMode { get; set; } // true if showing top-rated games instead of recommendations
    public List<RecommendedGame> Games { get; set; } = new();
}

public class RecommendedGame
{
    public Guid BoardGameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public decimal? Complexity { get; set; }
    public int? AveragePlaytimeMinutes { get; set; }
    public string? GameType { get; set; }
    public string? Theme { get; set; }
    public string? ComplexityTier { get; set; }
    public decimal? UserRating { get; set; } // Only set in fallback mode (user's rating)
}

