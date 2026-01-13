using IntegrationMocks.Core;
using IntegrationMocks.Core.Environments;
using IntegrationMocks.Core.Names;
using IntegrationMocks.Core.Networking;
using IntegrationMocks.Modules.MySql;
using MySqlConnector;

namespace Backend.Dotnet.Tests.TestHelpers;

public sealed class BackendServiceFixture : IAsyncLifetime
{
    public BackendServiceFixture()
    {
        var nameGenerator = new RandomNameGenerator("Test");

        MySql = new BindingInfrastructureService<MySqlServiceContract>(
            ServiceBinding.Create("BACKEND_TESTS_INFRASTRUCTURE_MYSQL", val =>
            {
                var builder = new MySqlConnectionStringBuilder(val);
                return new ExternalInfrastructureService<MySqlServiceContract>(new MySqlServiceContract
                {
                    Host = builder.Server,
                    Port = (int) builder.Port,
                    Username = builder.UserID,
                    Password = builder.Password
                });
            }),
            ServiceBinding.Create(() => new DockerMySqlService(
                nameGenerator,
                PortManager.Default,
                new DockerMySqlServiceOptions
                {
                    Image = "mysql:8.0"
                })));
        Backend = new BackendService(nameGenerator, PortManager.Default, MySql);
    }

    public IInfrastructureService<MySqlServiceContract> MySql { get; }

    public IInfrastructureService<BackendContract> Backend { get; }

    public async Task DisposeAsync()
    {
        if (Backend != null)
        {
            await Backend.DisposeAsync();
        }

        if (MySql != null)
        {
            await MySql.DisposeAsync();
        }
    }

    public async Task InitializeAsync()
    {
        await MySql.InitializeAsync();
        await Backend.InitializeAsync();
    }
}