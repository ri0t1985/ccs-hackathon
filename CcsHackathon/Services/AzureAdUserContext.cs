using System.Security.Claims;

namespace CcsHackathon.Services;

public class AzureAdUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AzureAdUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public string UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("oid")?.Value 
        ?? string.Empty;

    public string DisplayName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name
        ?? string.Empty;

    public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("preferred_username")?.Value
        ?? string.Empty;
}

