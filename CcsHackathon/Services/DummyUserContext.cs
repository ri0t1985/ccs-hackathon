namespace CcsHackathon.Services;

public class DummyUserContext : IUserContext
{
    public string UserId => "demo-user-id";
    public string DisplayName => "Demo User";
    public string Email => "demo@css-hackathon.local";
    public bool IsAuthenticated => true;
}

