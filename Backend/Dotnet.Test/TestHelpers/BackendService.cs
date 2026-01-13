using IntegrationMocks.Core;
using IntegrationMocks.Core.Names;
using IntegrationMocks.Core.Networking;
using IntegrationMocks.Modules.AspNetCore;
using IntegrationMocks.Modules.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
//using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Backend.Dotnet.Tests.TestHelpers;

public sealed class BackendService : WebApplicationService<BackendContract>
{
    private readonly IPort _controllerPort;
    private readonly string _databaseConnectionString;

    public BackendService(
        INameGenerator nameGenerator,
        IPortManager portManager,
        IInfrastructureService<MySqlServiceContract> mySql)
    {
        _controllerPort = portManager.TakePort();
        _databaseConnectionString = mySql.CreateMySqlConnectionString(nameGenerator.GenerateName());
        Contract = new BackendContract
        {
            ApiUrl = new Uri($"http://localhost:{_controllerPort.Number}")
        };
    }

    public override BackendContract Contract { get; }

    protected override void Configure(WebApplication app)
    {
        BackendStartup.Configure(app);
    }

    protected override WebApplicationBuilder CreateWebApplicationBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(opt =>
        {
            opt.ValidateOnBuild = true;
            opt.ValidateScopes = true;
        });
        builder.Configuration.AddJsonFile("appsettings.json").AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Kestrel:Endpoints:Http:Url"] = Contract.ApiUrl.ToString(),
            ["Database:ConnectionString"] = _databaseConnectionString
        });
        BackendStartup.ConfigureServices(builder.Services, builder.Configuration);
        return builder;
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        var version = ServerVersion.Create(Version.Parse(MySqlMeta.Version), ServerType.MySql);
        await using var usersDbContext = new UsersDbContext(
            new DbContextOptionsBuilder<UsersDbContext>().UseMySql(_databaseConnectionString, version).Options);
        await usersDbContext.Database.EnsureDeletedAsync();

        _controllerPort.Dispose();
        await base.DisposeAsync(disposing);
    }
}