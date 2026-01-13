using Backend.Dotnet.Tests.TestHelpers.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Dotnet.Tests.TestHelpers.Service;

internal sealed class BackendHealthTestClient(HttpClient inner) : IDisposable
{
    public const string HealthPath = "/api/v1/health";

    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<ApiResponse<HealthStatus>> GetAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _inner.GetAsync(HealthPath, cancellationToken);
        return await response.ToEnumApiResponseAsync<HealthStatus>(cancellationToken);
    }
}