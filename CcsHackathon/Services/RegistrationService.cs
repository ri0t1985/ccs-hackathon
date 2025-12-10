using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _dbContext;

    public RegistrationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Registration> CreateRegistrationAsync(string userId, string userDisplayName, string? foodRequirements, List<string> gameNames)
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserDisplayName = userDisplayName,
            FoodRequirements = foodRequirements,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var gameName in gameNames)
        {
            var gameRegistration = new GameRegistration
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                GameId = gameName
            };
            registration.GameRegistrations.Add(gameRegistration);
        }

        _dbContext.Registrations.Add(registration);
        await _dbContext.SaveChangesAsync();

        return registration;
    }
}

