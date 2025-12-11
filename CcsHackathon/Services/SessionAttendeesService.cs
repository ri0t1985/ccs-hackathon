using CcsHackathon.Data;
using Microsoft.EntityFrameworkCore;

namespace CcsHackathon.Services;

public class SessionAttendeesService : ISessionAttendeesService
{
    private readonly ApplicationDbContext _dbContext;

    public SessionAttendeesService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SessionAttendeesInfo?> GetSessionAttendeesAsync(Guid sessionId)
    {
        // Get the session
        var session = await _dbContext.Sessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && !s.IsCancelled);

        if (session == null)
        {
            return null;
        }

        // Get all registrations for this session
        var registrations = await _dbContext.Registrations
            .Include(r => r.GameRegistrations)
                .ThenInclude(gr => gr.BoardGame)
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.UserDisplayName)
            .ToListAsync();

        // Build attendee info
        var attendees = registrations
            .Select(r => new AttendeeInfo
            {
                UserDisplayName = r.UserDisplayName,
                BoardGames = r.GameRegistrations
                    .Select(gr => gr.BoardGame.Name)
                    .OrderBy(name => name)
                    .ToList(),
                FoodRequirements = r.FoodRequirements
            })
            .ToList();

        return new SessionAttendeesInfo
        {
            Session = session,
            Attendees = attendees
        };
    }
}

