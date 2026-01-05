using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Cookies;

/// <summary>
/// Cookie-based implementation of <see cref="ITokenAccessor"/> that reads and writes
/// the access token to an HTTP cookie.
/// </summary>
/// <param name="options">Configuration options that provide cookie names and settings.</param>
public sealed class CookieTokenAccessor(IOptions<CookieTokenAccessorOptions> options) : ITokenAccessor
{
    private readonly IOptions<CookieTokenAccessorOptions> _options = options;

    /// <summary>
    /// Retrieves the access token from the incoming HTTP request cookies.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest"/> to read the cookie from.</param>
    /// <returns>The access token value if the cookie exists; otherwise, <c>null</c>.</returns>
    public string? GetAccessTokenOrDefault(HttpRequest request)
    {
        return request.Cookies[_options.Value.AccessTokenCookieName];
    }

    /// <summary>
    /// Sets the access token cookie on the outgoing HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponse"/> to append the cookie to.</param>
    /// <param name="token">The access token value to store in the cookie.</param>
    /// <remarks>
    /// The cookie is appended during response start via <see cref="HttpResponse.OnStarting"/>
    /// and uses secure defaults (HttpOnly, Secure, SameSite=Lax).
    /// </remarks>
    public void SetAccessToken(HttpResponse response, string token)
    {
        response.OnStarting(() =>
        {
            response.Cookies.Append(_options.Value.AccessTokenCookieName, token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Deletes the access token cookie from the response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponse"/> to remove the cookie from.</param>
    public void DeleteTokens(HttpResponse response)
    {
        response.Cookies.Delete(_options.Value.AccessTokenCookieName);
    }
}