namespace CcsHackathon.Services;

public interface IUserContext
{
    string UserId { get; }
    string DisplayName { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}

