using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.Dotnet.Controller.Users.Models;

namespace Backend.Dotnet.Tests.TestHelpers.Users;

internal sealed class BackendUsersTestClient(HttpClient inner) : IDisposable
{
    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<ApiResponse<UserView>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _inner.GetAsync("/api/v1/users/current", cancellationToken);
        return await response.ToJsonApiResponseAsync<UserView>(cancellationToken);
    }
}