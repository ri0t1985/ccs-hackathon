using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace CcsHackathon.Services;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphServiceClient;

    public GraphService(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<IEnumerable<Microsoft.Graph.Models.User>> GetUsersAsync()
    {
        try
        {
            var users = await _graphServiceClient.Users.GetAsync();
            return users?.Value ?? Enumerable.Empty<Microsoft.Graph.Models.User>();
        }
        catch
        {
            return Enumerable.Empty<Microsoft.Graph.Models.User>();
        }
    }
}

