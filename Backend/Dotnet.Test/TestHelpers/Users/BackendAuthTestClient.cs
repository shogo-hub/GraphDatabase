using Backend.Common.Miscellaneous;
using Backend.Common.Serialization.Json;
using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.WebApi.Users.Models;
using System.Net.Http.Json;

namespace Backend.Dotnet.Tests.TestHelpers.Users;

internal sealed class BackendAuthTestClient(HttpClient inner) : IDisposable
{
    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<ApiResponse<UserView>> RegisterAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _inner.PostAsJsonAsync(
            "/api/v1/auth/register",
            request,
            WebApiJsonSerializer.Options,
            cancellationToken);
        return await response.ToJsonApiResponseAsync<UserView>(cancellationToken);
    }

    public async Task<ApiResponse<Unit>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _inner.PostAsJsonAsync(
            "/api/v1/auth/login",
            request,
            WebApiJsonSerializer.Options,
            cancellationToken);
        return await response.ToEmptyApiResponseAsync(cancellationToken);
    }

    public async Task<ApiResponse<Unit>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _inner.PostAsync("/api/v1/auth/logout", null, cancellationToken);
        return await response.ToEmptyApiResponseAsync(cancellationToken);
    }
}