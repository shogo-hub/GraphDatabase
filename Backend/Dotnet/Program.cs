using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System;

namespace Backend.Dotnet;

internal static class Program
{
    private static async Task Main(string[] _)
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddJsonFile("appsettings.json").AddEnvironmentVariables();
        builder.Host.UseDefaultServiceProvider(opt =>
        {
            opt.ValidateOnBuild = true;
            opt.ValidateScopes = true;
        });
        // Set detail setting for DI
        BackendDetailSetting.ConfigureServices(builder.Services, builder.Configuration);
        await using var host = builder.Build();
        BackendDetailSetting.Configure(host);
        await host.RunAsync();
    }
}


internal static class BackendDetailSetting
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddControllers(configuration)
            .AddDatabase(configuration)
            .AddInitialization();
    }

    public static void Configure(WebApplication app)
    {
        app.UseRouting();
        app.UseHealthChecks(HealthController.GetPath);
        app.MapOpenApi(OpenApiController.GetPath);
        app.UseSwaggerUI(opt =>
        {
            opt.SwaggerEndpoint(OpenApiController.GetPath, OpenApiController.GetPath);
        });
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}