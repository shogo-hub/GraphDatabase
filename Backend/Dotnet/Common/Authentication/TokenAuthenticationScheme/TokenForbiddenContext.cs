using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Backend.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Context provided to handlers when access is forbidden (authorization failed).
/// Contains the current <see cref="HttpContext"/>, authentication scheme,
/// options, and any optional <see cref="AuthenticationProperties"/>.
/// </summary>
/// <param name="context">The current HTTP context.</param>
/// <param name="scheme">The authentication scheme for the request.</param>
/// <param name="options">The <see cref="TokenAuthenticationOptions"/> in use.</param>
/// <param name="properties">Optional authentication properties associated with the request.</param>
public sealed class TokenForbiddenContext(
    HttpContext context,
    AuthenticationScheme scheme,
    TokenAuthenticationOptions options,
    AuthenticationProperties? properties) :
    PropertiesContext<TokenAuthenticationOptions>(context, scheme, options, properties)
{
    /// <summary>
    /// When set to <c>true</c> by an event handler, indicates the forbidden
    /// condition has been handled and no further processing should occur.
    /// </summary>
    public bool Handled { get; set; }
}