using Backend.Dotnet.Application.AIChat.PromptCreator;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenAI;
using Backend.Dotnet.Application.AIChat.AIModelProvider.Mock;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenRouter;
using Backend.Dotnet.Application.AIChat.Configuration;
using Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;
using Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme.Paseto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Backend.Dotnet.Controllers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddControllers(this IServiceCollection services, IConfiguration configuration)
    {
        // Authentication
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = PasetoTokenCookieDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = PasetoTokenCookieDefaults.AuthenticationScheme;
        });

        // AI Integration - Register AI services
        services.Configure<AIChatOptions>(configuration.GetSection("AIChat"));

        // Register prompt template service
        services.AddSingleton<IPromptTemplateService, FileBasedPromptTemplateService>();

        // Register AI provider factory
        services.AddSingleton<AIProviderFactory>();

        // Register Mock AI client
        services.AddSingleton<IAiClient, MockTestClient>();

        // Register typed HttpClient for OpenAI
        services.AddHttpClient<OpenAiClient>((sp, client) =>
        {
            var aiChatOptions = sp.GetRequiredService<IOptions<AIChatOptions>>().Value;
            var providerInfo = aiChatOptions.GetRequiredProvider("OpenAi");
            
            client.BaseAddress = new Uri(providerInfo.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(providerInfo.TimeoutSeconds);

            if (!string.IsNullOrEmpty(providerInfo.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", providerInfo.ApiKey);
            }
        });

        // Register OpenAiClient as IAiClient
        services.AddSingleton<IAiClient>(sp => sp.GetRequiredService<OpenAiClient>());

        // Register typed HttpClient for OpenRouter
        services.AddHttpClient<OpenRouterClient>((sp, client) =>
        {
            var aiChatOptions = sp.GetRequiredService<IOptions<AIChatOptions>>().Value;
            var providerInfo = aiChatOptions.GetRequiredProvider("OpenRouter");
            
            client.BaseAddress = new Uri(providerInfo.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(providerInfo.TimeoutSeconds);

            if (!string.IsNullOrEmpty(providerInfo.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", providerInfo.ApiKey);

                // OpenRouter-specific headers
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
                client.DefaultRequestHeaders.Add("X-Title", "GraphDatabase-Backend");
            }
        });

        // Register OpenRouterClient as IAiClient
        services.AddSingleton<IAiClient>(sp => sp.GetRequiredService<OpenRouterClient>());

        return services;
    }
}