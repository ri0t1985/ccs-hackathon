using Azure.Core;
using Microsoft.Identity.Web;

namespace CcsHackathon;

// Helper class to bridge ITokenAcquisition to TokenCredential
public class TokenCredentialWrapper : TokenCredential
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly string[] _scopes;

    public TokenCredentialWrapper(ITokenAcquisition tokenAcquisition, string[] scopes)
    {
        _tokenAcquisition = tokenAcquisition;
        _scopes = scopes;
    }

    public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var token = _tokenAcquisition.GetAccessTokenForUserAsync(_scopes).GetAwaiter().GetResult();
        return new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1));
    }

    public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
    {
        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(_scopes);
        return new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1));
    }
}

