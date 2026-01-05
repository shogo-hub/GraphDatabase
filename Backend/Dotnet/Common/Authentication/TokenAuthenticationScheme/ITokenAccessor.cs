using Microsoft.AspNetCore.Http;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Interface to Get token from response
/// </summary>
public interface ITokenAccessor
{
    /// <summary>
    /// Retrieves the access token from the HTTP request if present.
    /// </summary>
    /// <param name="request">The HTTP request containing the token.</param>
    /// <returns>The access token string if found; otherwise, null.</returns>
    string? GetAccessTokenOrDefault(HttpRequest request);

    /// <summary>
    /// Sets the access token in the HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response to set the token on.</param>
    /// <param name="token">The access token to set.</param>
    void SetAccessToken(HttpResponse response, string token);

    /// <summary>
    /// Removes all authentication tokens from the HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response to remove tokens from.</param>
    void DeleteTokens(HttpResponse response);
}