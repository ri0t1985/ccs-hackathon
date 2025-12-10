using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IBoardGameOverviewService
{
    Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null, string? userId = null);
}

public record BoardGameOverviewItem
{
    public string GameName { get; init; } = string.Empty;
    public string? Summary { get; init; }
    public decimal? Complexity { get; init; }
    public int? TimeToSetupMinutes { get; init; }
    public bool HasAiData { get; init; }
    public DateTime LastUpdatedAt { get; init; }
    // Session-specific data (only populated when sessionId is provided)
    public int ParticipantCount { get; init; } = 0;
    public bool IsUserParticipating { get; init; } = false;
}

