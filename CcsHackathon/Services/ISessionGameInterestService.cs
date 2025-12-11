namespace CcsHackathon.Services;

public interface ISessionGameInterestService
{
    Task<bool> AddInterestAsync(string userId, Guid sessionId, Guid boardGameId);
    Task<bool> RemoveInterestAsync(string userId, Guid sessionId, Guid boardGameId);
    Task<bool> HasInterestAsync(string userId, Guid sessionId, Guid boardGameId);
    Task<Dictionary<Guid, int>> GetInterestCountsAsync(Guid sessionId, List<Guid> boardGameIds);
    Task<HashSet<Guid>> GetUserInterestsAsync(string userId, Guid sessionId, List<Guid> boardGameIds);
}

