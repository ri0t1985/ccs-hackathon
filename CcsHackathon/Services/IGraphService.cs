namespace CcsHackathon.Services;

public interface IGraphService
{
    Task<IEnumerable<Microsoft.Graph.Models.User>> GetUsersAsync();
}

