using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class GameRatingService : IGameRatingService
{
    private readonly ApplicationDbContext _dbContext;

    public GameRatingService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GameRating?> GetRatingAsync(string userId, Guid boardGameId, Guid sessionId)
    {
        return await _dbContext.GameRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BoardGameId == boardGameId && r.SessionId == sessionId);
    }

    public async Task<Dictionary<Guid, int>> GetRatingsForSessionAsync(string userId, Guid sessionId)
    {
        var ratings = await _dbContext.GameRatings
            .Where(r => r.UserId == userId && r.SessionId == sessionId)
            .ToListAsync();

        return ratings.ToDictionary(r => r.BoardGameId, r => r.Rating);
    }

    public async Task<GameRating> SaveRatingAsync(string userId, Guid boardGameId, Guid sessionId, int rating)
    {
        // Validate rating range
        if (rating < 0 || rating > 5)
        {
            throw new ArgumentException("Rating must be between 0 and 5", nameof(rating));
        }

        var existingRating = await GetRatingAsync(userId, boardGameId, sessionId);

        if (existingRating != null)
        {
            // Update existing rating
            existingRating.Rating = rating;
            existingRating.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return existingRating;
        }
        else
        {
            // Create new rating
            var newRating = new GameRating
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BoardGameId = boardGameId,
                SessionId = sessionId,
                Rating = rating,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.GameRatings.Add(newRating);
            await _dbContext.SaveChangesAsync();
            return newRating;
        }
    }

    public async Task<Dictionary<Guid, decimal>> GetAverageRatingsAsync(IEnumerable<Guid> boardGameIds)
    {
        var boardGameIdsList = boardGameIds.ToList();
        if (!boardGameIdsList.Any())
        {
            return new Dictionary<Guid, decimal>();
        }

        var averageRatings = await _dbContext.GameRatings
            .Where(r => boardGameIdsList.Contains(r.BoardGameId))
            .GroupBy(r => r.BoardGameId)
            .Select(g => new
            {
                BoardGameId = g.Key,
                AverageRating = g.Average(r => (decimal)r.Rating),
                RatingCount = g.Count()
            })
            .ToDictionaryAsync(x => x.BoardGameId, x => x.AverageRating);

        return averageRatings;
    }
}

