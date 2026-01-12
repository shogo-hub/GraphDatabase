using Backend.Dotnet.Application.AIChat;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Application.AIChat.PromptCreator;
using Backend.Dotnet.Common.Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Dotnet.Application;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services
            .AddScoped<IAIChatService, AIChatService>()
            .AddScoped<AIProviderFactory>()
            .AddScoped<IPromptTemplateService, FileBasedPromptTemplateService>()
            .AddMediator();
    }
}