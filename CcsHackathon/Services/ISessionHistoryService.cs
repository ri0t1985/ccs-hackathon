using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface ISessionHistoryService
{
    Task<IEnumerable<SessionHistoryItem>> GetSessionHistoryAsync(string userId, DateOnly? fromDate = null, DateOnly? toDate = null, string? gameNameFilter = null);
}

public class SessionHistoryItem
{
    public Session Session { get; set; } = null!;
    public int AttendeeCount { get; set; }
    public List<GameHistoryItem> Games { get; set; } = new();
}

public class GameHistoryItem
{
    public Guid BoardGameId { get; set; }
    public string GameName { get; set; } = string.Empty;
    public int? UserRating { get; set; } // User's rating (0-5) or null if not rated
    public decimal? AverageRating { get; set; } // Average rating across all users or null if no ratings
    public int RatingCount { get; set; } // Number of ratings for this game
}

