using Backend.Dotnet.Tests.TestHelpers;
using Backend.Dotnet.Tests.TestHelpers.Http;
using Backend.Dotnet.Tests.TestHelpers.Service;
using IntegrationMocks.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Backend.Dotnet.Tests.Component.Service;

public sealed class HealthTests : IClassFixture<BackendServiceFixture>, IDisposable
{
    private readonly IInfrastructureService<BackendContract> _backend;
    private readonly BackendHealthTestClient _healthClient;

    public HealthTests(BackendServiceFixture backendFixture)
    {
        _backend = backendFixture.Backend;
        _healthClient = new BackendHealthTestClient(TestHttpClientFactory.Create(_backend.Contract.ApiUrl));
    }

    public void Dispose()
    {
        _healthClient.Dispose();
    }

    [Fact]
    public async Task GetAsync_returns_healthy_status()
    {
        var status = await _healthClient.GetAsync();

        Assert.Equal(HealthStatus.Healthy, status.GetValueOrThrow());
    }
}