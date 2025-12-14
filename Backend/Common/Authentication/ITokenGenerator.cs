using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Backend.Common.Authentication.TokenAuthenticationScheme;

public interface ITokenGenerator
{
    Task<TokenGenerationResult> GenerateTokensAsync(
        ClaimsPrincipal user,
        AuthenticationProperties? properties,
        CancellationToken cancellationToken);
}