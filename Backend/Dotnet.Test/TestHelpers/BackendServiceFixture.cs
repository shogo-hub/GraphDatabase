using IntegrationMocks.Core;
using IntegrationMocks.Core.Environments;
using IntegrationMocks.Core.Names;
using IntegrationMocks.Core.Networking;


namespace Backend.Dotnet.Tests.TestHelpers;

public sealed class BackendServiceFixture : IAsyncLifetime
{
    public BackendServiceFixture()
    {
        var nameGenerator = new RandomNameGenerator("Test");

        // Tests don't start MySQL; omit infrastructure binding and pass only ports
        Backend = new BackendService(nameGenerator, PortManager.Default);
    }


    public IInfrastructureService<BackendContract> Backend { get; }

    public async Task DisposeAsync()
    {
        if (Backend != null)
        {
            await Backend.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await Backend.InitializeAsync();
    }

}