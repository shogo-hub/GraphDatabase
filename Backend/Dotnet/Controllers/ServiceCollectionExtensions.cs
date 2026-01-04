using Backend.Application.AIChat.PromptCreator;
using Backend.Application.AIChat.AIModelProvider;
using Backend.Application.AIChat.AIModelProvider.OpenAI;
using Backend.Application.AIChat.AIModelProvider.Mock;
using Backend.Application.AIChat.AIModelProvider.OpenRouter;
using Backend.Common.Authentication.TokenAuthenticationScheme;
using Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;
using Backend.Common.Errors.AspNetCore;
@@ -9,6 +14,8 @@
using Backend.WebApi.Users.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Backend.WebApi;

@@ -81,6 +88,57 @@ public static IServiceCollection AddWebApi(this IServiceCollection services, ICo
                opt.DefaultChallengeScheme = PasetoTokenCookieDefaults.AuthenticationScheme;
            });

        // AI Integration - Register AI services
        services.Configure<AiOptions>(configuration.GetSection("AI"));
        services.Configure<AiOptions >(configuration.GetSection("OpenRouter"));

        // Register prompt template service
        services.AddSingleton<IPromptTemplateService, FileBasedPromptTemplateService>();

        // Register AI provider factory
        services.AddSingleton<AIProviderFactory>();

        // Register Mock AI client
        services.AddSingleton<IAiClient, MockTestClient>();

        // Register typed HttpClient for OpenAI
        services.AddHttpClient<OpenAiClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AiOptions>>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);

            if (!string.IsNullOrEmpty(opts.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", opts.ApiKey);
            }
        });

        // Register OpenAiClient as IAiClient
        services.AddSingleton<IAiClient>(sp => sp.GetRequiredService<OpenAiClient>());

        // Register typed HttpClient for OpenRouter (Test)
        services.AddHttpClient<TestClient>((sp, client) =>
        {
            var opts = sp.GetRequiredService<IOptions<AiOptions >>().Value;
            client.BaseAddress = new Uri(opts.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);

            if (!string.IsNullOrEmpty(opts.ApiKey))
            {
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", opts.ApiKey);

                // OpenRouter-specific headers
                client.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
                client.DefaultRequestHeaders.Add("X-Title", "CT-Backend-Test");
            }
        });

        // Register TestClient as IAiClient
        services.AddSingleton<IAiClient>(sp => sp.GetRequiredService<TestClient>());

        return services;
    }
}