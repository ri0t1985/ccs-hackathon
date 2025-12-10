using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameOverviewService
{
    Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null);
}

public record BoardGameOverviewItem
{
    public string GameName { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public decimal? Complexity { get; init; }
    public int? TimeToSetupMinutes { get; init; }
    public bool HasAiData { get; init; }
    public DateTime LastUpdatedAt { get; init; }
}

