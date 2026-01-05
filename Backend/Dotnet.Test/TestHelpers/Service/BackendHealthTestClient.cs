using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.Dotnet.Controller.Service;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Dotnet.Tests.TestHelpers.Service;

internal sealed class BackendHealthTestClient(HttpClient inner) : IDisposable
{
    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<ApiResponse<HealthStatus>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _inner.GetAsync(HealthController.GetPath, cancellationToken);
        return await response.ToEnumApiResponseAsync<HealthStatus>(cancellationToken);
    }
}