using Microsoft.AspNetCore.Authentication;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Options for the token authentication scheme.
/// </summary>
public sealed class TokenAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Strongly-typed events for the token authentication scheme.
    /// </summary>
    public new TokenAuthenticationEvents Events
    {
        get => (TokenAuthenticationEvents)base.Events!;
        set => base.Events = value;
    }
}