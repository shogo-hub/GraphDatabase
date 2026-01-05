using Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Cookies;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Backend.Dotnet.Tests.TestHelpers.Http;

public static class TestHttpClientFactory
{
    public static TestHttpClient Create(Uri baseAddress)
    {
        return new TestHttpClient(
            new CookieHttpClientHandler
            {
                AllowAutoRedirect = false
            })
        {
            BaseAddress = baseAddress
        };
    }

    public static TestHttpClient Create(Uri baseAddress, string accessToken)
    {
        var httpClient = Create(baseAddress);
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation(
            HeaderNames.Cookie,
            $"{AccessTokenCookie.DefaultName}={accessToken}");
        return httpClient;
    }

    private class CookieHttpClientHandler : HttpClientHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            SetCookies(request);
            return base.Send(request, cancellationToken);
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            SetCookies(request);
            return await base.SendAsync(request, cancellationToken);
        }

        private void SetCookies(HttpRequestMessage request)
        {
            foreach (Cookie cookie in CookieContainer.GetAllCookies())
            {
                request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, $"{cookie.Name}={cookie.Value}");
            }
        }
    }
}