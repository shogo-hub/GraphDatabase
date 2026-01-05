using Backend.Dotnet.Common.Serialization.Json;
using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.Dotnet.Controllers.Users.Models;
using System.Net.Http.Json;

namespace Backend.Dotnet.Tests.TestHelpers.Users;

internal sealed class BackendSchoolsTestClient(HttpClient inner) : IDisposable
{
    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<ApiResponse<SchoolView>> CreateAsync(
        CreateSchoolRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _inner.PostAsJsonAsync(
            "/api/v1/schools",
            request,
            ControllerApiJsonSerializer.Options,
            cancellationToken);
        return await response.ToJsonApiResponseAsync<SchoolView>(cancellationToken);
    }
}