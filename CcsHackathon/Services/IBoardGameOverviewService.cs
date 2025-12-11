using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameOverviewService
{
    Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null);
}

public record BoardGameOverviewItem
{
    public Guid BoardGameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public decimal? Complexity { get; init; }
    public int? TimeToSetupMinutes { get; init; }
    public int? AveragePlaytimeMinutes { get; init; }
    public bool HasAiData { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    public decimal? AverageRating { get; init; }
    public int RatingCount { get; init; }
}

