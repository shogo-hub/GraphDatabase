using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Context passed to token signing-in event handlers. Provides access to the
/// current <see cref="HttpContext"/>, authentication scheme, options,
/// optional <see cref="AuthenticationProperties"/>, and the <see cref="ClaimsPrincipal"/>
/// that is being signed in.
/// </summary>
/// <param name="context">The current HTTP context.</param>
/// <param name="scheme">The authentication scheme for the request.</param>
/// <param name="options">The <see cref="TokenAuthenticationOptions"/> in use.</param>
/// <param name="properties">Optional authentication properties for the sign-in.</param>
/// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user who is signing in.</param>
public sealed class TokenSigningInContext(
    HttpContext context,
    AuthenticationScheme scheme,
    TokenAuthenticationOptions options,
    AuthenticationProperties? properties,
    ClaimsPrincipal user) :
    PropertiesContext<TokenAuthenticationOptions>(context, scheme, options, properties)
{
    /// <summary>
    /// The <see cref="ClaimsPrincipal"/> that is signing in. Handlers may
    /// inspect or replace this value before the sign-in completes.
    /// </summary>
    public ClaimsPrincipal User { get; set; } = user;

    /// <summary>
    /// When set to <c>true</c> by an event handler, indicates that the
    /// signing-in event has been handled and no further processing should occur.
    /// </summary>
    public bool Handled { get; set; }
}