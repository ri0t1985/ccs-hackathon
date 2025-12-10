using Microsoft.Graph.Models;

namespace CcsHackathon.Services;

public class DummyGraphService : IGraphService
{
    public Task<IEnumerable<User>> GetUsersAsync()
    {
        // Return empty list in dummy mode - no real Graph calls
        return Task.FromResult(Enumerable.Empty<User>());
    }
}

