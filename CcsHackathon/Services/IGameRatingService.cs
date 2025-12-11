using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IGameRatingService
{
    Task<GameRating?> GetRatingAsync(string userId, Guid boardGameId, Guid sessionId);
    Task<GameRating> SaveRatingAsync(string userId, Guid boardGameId, Guid sessionId, int rating);
    Task<Dictionary<Guid, int>> GetRatingsForSessionAsync(string userId, Guid sessionId);
}

