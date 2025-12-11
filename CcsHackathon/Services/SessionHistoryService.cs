using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class SessionHistoryService : ISessionHistoryService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGameRatingService _ratingService;

    public SessionHistoryService(ApplicationDbContext dbContext, IGameRatingService ratingService)
    {
        _dbContext = dbContext;
        _ratingService = ratingService;
    }

    public async Task<IEnumerable<SessionHistoryItem>> GetSessionHistoryAsync(string userId, DateOnly? fromDate = null, DateOnly? toDate = null, string? gameNameFilter = null)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        // Query past sessions
        var sessionsQuery = _dbContext.Sessions
            .Where(s => !s.IsCancelled && s.Date < today);

        // Apply date filters
        if (fromDate.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Date >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Date <= toDate.Value);
        }

        // Order by most recent first
        var sessions = await sessionsQuery
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        var sessionIds = sessions.Select(s => s.Id).ToList();

        // Get attendee counts per session
        var attendeeCounts = await _dbContext.Registrations
            .Where(r => r.SessionId.HasValue && sessionIds.Contains(r.SessionId.Value))
            .GroupBy(r => r.SessionId!.Value)
            .Select(g => new { SessionId = g.Key, Count = g.Select(r => r.UserId).Distinct().Count() })
            .ToDictionaryAsync(x => x.SessionId, x => x.Count);

        // Get all games for these sessions
        var gameRegistrations = await _dbContext.GameRegistrations
            .Include(gr => gr.Registration)
            .Include(gr => gr.BoardGame)
            .Where(gr => gr.Registration.SessionId.HasValue && sessionIds.Contains(gr.Registration.SessionId.Value))
            .ToListAsync();

        // Apply game name filter if provided
        if (!string.IsNullOrWhiteSpace(gameNameFilter))
        {
            var filterLower = gameNameFilter.ToLowerInvariant();
            gameRegistrations = gameRegistrations
                .Where(gr => gr.BoardGame.Name.ToLowerInvariant().Contains(filterLower))
                .ToList();
        }

        // Group games by session and board game
        var gamesBySession = gameRegistrations
            .GroupBy(gr => new { SessionId = gr.Registration.SessionId!.Value, BoardGameId = gr.BoardGameId })
            .Select(g => new
            {
                g.Key.SessionId,
                g.Key.BoardGameId,
                GameName = g.First().BoardGame.Name
            })
            .GroupBy(x => x.SessionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Get all ratings for the user for these sessions
        var allRatings = new Dictionary<Guid, Dictionary<Guid, int>>();
        foreach (var sessionId in sessionIds)
        {
            var ratings = await _ratingService.GetRatingsForSessionAsync(userId, sessionId);
            allRatings[sessionId] = ratings;
        }

        // Build result
        var result = new List<SessionHistoryItem>();
        foreach (var session in sessions)
        {
            var games = gamesBySession.ContainsKey(session.Id)
                ? gamesBySession[session.Id]
                    .Select(g => new GameHistoryItem
                    {
                        BoardGameId = g.BoardGameId,
                        GameName = g.GameName,
                        UserRating = allRatings.ContainsKey(session.Id) && allRatings[session.Id].ContainsKey(g.BoardGameId)
                            ? allRatings[session.Id][g.BoardGameId]
                            : null
                    })
                    .OrderBy(g => g.GameName)
                    .ToList()
                : new List<GameHistoryItem>();

            // Only include sessions that have games (or if no game filter is applied)
            if (games.Any() || string.IsNullOrWhiteSpace(gameNameFilter))
            {
                result.Add(new SessionHistoryItem
                {
                    Session = session,
                    AttendeeCount = attendeeCounts.GetValueOrDefault(session.Id, 0),
                    Games = games
                });
            }
        }

        return result;
    }
}

