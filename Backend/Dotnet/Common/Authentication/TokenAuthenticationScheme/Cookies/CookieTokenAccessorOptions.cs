namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Cookies;

/// <summary>
/// Configuration options for <see cref="CookieTokenAccessor"/>.
/// </summary>
public sealed class CookieTokenAccessorOptions
{
    /// <summary>
    /// The name of the cookie used to store the access token.
    /// </summary>
    public string AccessTokenCookieName { get; set; } = AccessTokenCookie.DefaultName;
}