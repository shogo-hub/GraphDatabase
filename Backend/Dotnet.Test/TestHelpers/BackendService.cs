using IntegrationMocks.Core;
using IntegrationMocks.Core.Names;
using IntegrationMocks.Core.Networking;
using IntegrationMocks.Modules.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Backend.Dotnet.Controllers;

namespace Backend.Dotnet.Tests.TestHelpers;

public sealed class BackendService : WebApplicationService<BackendContract>
{
    private readonly IPort _controllerPort;
    private readonly string _databaseConnectionString;

    public BackendService(
        INameGenerator nameGenerator,
        IPortManager portManager)
    {
        _controllerPort = portManager.TakePort();
        _databaseConnectionString = string.Empty; // no MySQL in tests by default
        Contract = new BackendContract
        {
            ApiUrl = new Uri($"http://localhost:{_controllerPort.Number}")
        };
    }

    public override BackendContract Contract { get; }

    protected override void Configure(WebApplication app)
    {
        Dotnet.BackendDetailSetting.Configure(app);
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
        Backend.Dotnet.BackendDetailSetting.ConfigureServices(builder.Services, builder.Configuration);
        // Use project-defined AddControllers(this IServiceCollection, IConfiguration)
        builder.Services.AddControllers(builder.Configuration);

        // Ensure controllers assembly is included
        builder.Services.AddControllers()
            .AddApplicationPart(typeof(Backend.Dotnet.Controllers.Service.AIChat.AIChatController).Assembly);

        return builder;
    }

    protected override async ValueTask DisposeAsync(bool disposing)
    {
        // Database cleanup - uncomment if UsersDbContext exists
        // var version = ServerVersion.AutoDetect(_databaseConnectionString);
        // await using var usersDbContext = new UsersDbContext(
        //     new DbContextOptionsBuilder<UsersDbContext>().UseMySql(_databaseConnectionString, version).Options);
        // await usersDbContext.Database.EnsureDeletedAsync();

        _controllerPort.Dispose();
        await base.DisposeAsync(disposing);
    }
}