using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Backend.Common.Authentication.TokenAuthenticationScheme;

public sealed class TokenChallengeContext(
    HttpContext context,
    AuthenticationScheme scheme,
    TokenAuthenticationOptions options,
    AuthenticationProperties? properties) :
    PropertiesContext<TokenAuthenticationOptions>(context, scheme, options, properties)
{
    public bool Handled { get; set; }
}