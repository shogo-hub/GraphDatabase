namespace Backend.Common.Authentication.TokenAuthenticationScheme.Cookies;

/// <summary>
/// Provides defaults for the access token cookie used by the cookie-based token accessor.
/// </summary>
public sealed class AccessTokenCookie
{
    /// <summary>
    /// The default cookie name used to store the access token.
    /// </summary>
    public const string DefaultName = "auth_token";
}