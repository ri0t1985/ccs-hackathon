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

    public async Task<Registration> CreateRegistrationAsync(string userId, string userDisplayName, string? foodRequirements, List<string> gameNames, Guid? sessionId = null)
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserDisplayName = userDisplayName,
            FoodRequirements = foodRequirements,
            CreatedAt = DateTime.UtcNow,
            SessionId = sessionId
        };

        foreach (var gameName in gameNames)
        {
            // Find or create BoardGame by name
            var boardGame = await _dbContext.BoardGames
                .FirstOrDefaultAsync(bg => bg.Name == gameName);

            if (boardGame == null)
            {
                boardGame = new BoardGame
                {
                    Id = Guid.NewGuid(),
                    Name = gameName,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.BoardGames.Add(boardGame);
            }

            var gameRegistration = new GameRegistration
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                BoardGameId = boardGame.Id
            };
            registration.GameRegistrations.Add(gameRegistration);
        }

        _dbContext.Registrations.Add(registration);
        await _dbContext.SaveChangesAsync();

        return registration;
    }
}

