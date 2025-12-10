using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CcsHackathon.Services;

public class DummyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DummyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "demo-user-id"),
            new Claim(ClaimTypes.Name, "Demo User"),
            new Claim(ClaimTypes.Email, "demo@css-hackathon.local"),
            new Claim("oid", "demo-user-id")
        };

        var identity = new ClaimsIdentity(claims, "Dummy");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Dummy");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

