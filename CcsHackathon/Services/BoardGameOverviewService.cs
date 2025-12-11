using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class BoardGameOverviewService : IBoardGameOverviewService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IGameRatingService _ratingService;
    private readonly ISessionGameInterestService _interestService;

    public BoardGameOverviewService(
        ApplicationDbContext dbContext, 
        IGameRatingService ratingService,
        ISessionGameInterestService interestService)
    {
        _dbContext = dbContext;
        _ratingService = ratingService;
        _interestService = interestService;
    }

    public async Task<IEnumerable<BoardGameOverviewItem>> GetBoardGamesAsync(Guid? sessionId = null, string? userId = null)
    {
        // Require sessionId - only return games for a specific session
        if (!sessionId.HasValue)
        {
            return Enumerable.Empty<BoardGameOverviewItem>();
        }

        // Check if session is upcoming
        var session = await _dbContext.Sessions.FindAsync(sessionId.Value);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isUpcoming = session != null && !session.IsCancelled && session.Date >= today;

        // Get board games from GameRegistrations that are registered for the specified session
        // Query: GameRegistration -> Registration -> SessionId
        // Ensure we filter by the exact SessionId match
        var gameRegistrations = await _dbContext.GameRegistrations
            .Include(gr => gr.Registration)
            .Include(gr => gr.BoardGame)
            .Include(gr => gr.BoardGameCache)
            .Where(gr => gr.Registration.SessionId != null && gr.Registration.SessionId.Value == sessionId.Value)
            .ToListAsync();

        if (!gameRegistrations.Any())
        {
            return Enumerable.Empty<BoardGameOverviewItem>();
        }

        // Group by BoardGameId to get unique games, preferring entries with AI data
        var uniqueGames = gameRegistrations
            .GroupBy(gr => gr.BoardGameId)
            .Select(g => g.OrderByDescending(gr => gr.BoardGameCache?.HasAiData ?? false).First())
            .ToList();

        // Get average ratings for all games
        var boardGameIds = uniqueGames.Select(g => g.BoardGameId).ToList();
        var averageRatings = await _ratingService.GetAverageRatingsAsync(boardGameIds);

        // Get rating counts for all games
        var ratingCounts = await _dbContext.GameRatings
            .Where(r => boardGameIds.Contains(r.BoardGameId))
            .GroupBy(r => r.BoardGameId)
            .Select(g => new { BoardGameId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.BoardGameId, x => x.Count);

        // Get interest counts and user interests (only for upcoming sessions)
        Dictionary<Guid, int> interestCounts = new();
        HashSet<Guid> userInterests = new();
        
        if (isUpcoming)
        {
            interestCounts = await _interestService.GetInterestCountsAsync(sessionId.Value, boardGameIds);
            
            if (!string.IsNullOrWhiteSpace(userId))
            {
                userInterests = await _interestService.GetUserInterestsAsync(userId, sessionId.Value, boardGameIds);
            }
        }

        // Build result
        var games = uniqueGames
            .Select(gr => new BoardGameOverviewItem
            {
                BoardGameId = gr.BoardGameId,
                GameName = gr.BoardGame.Name,
                Summary = gr.BoardGameCache?.Summary,
                Complexity = gr.BoardGameCache?.Complexity,
                TimeToSetupMinutes = gr.BoardGameCache?.TimeToSetupMinutes,
                AveragePlaytimeMinutes = gr.BoardGame.AveragePlaytimeMinutes,
                HasAiData = gr.BoardGameCache?.HasAiData ?? false,
                LastUpdatedAt = gr.BoardGameCache?.LastUpdatedAt ?? gr.Registration.CreatedAt,
                AverageRating = averageRatings.ContainsKey(gr.BoardGameId) ? averageRatings[gr.BoardGameId] : null,
                RatingCount = ratingCounts.GetValueOrDefault(gr.BoardGameId, 0),
                InterestCount = interestCounts.GetValueOrDefault(gr.BoardGameId, 0),
                UserHasInterest = userInterests.Contains(gr.BoardGameId),
                IsUpcomingSession = isUpcoming
            })
            .OrderBy(g => g.GameName)
            .ToList();

        return games;
    }
}

