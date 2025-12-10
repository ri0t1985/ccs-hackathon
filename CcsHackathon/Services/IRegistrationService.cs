using CcsHackathon.Data;

namespace CcsHackathon.Services;

public interface IRegistrationService
{
    Task<Registration> CreateRegistrationAsync(string userId, string userDisplayName, string? foodRequirements, List<string> gameNames, Guid? sessionId = null);
}

