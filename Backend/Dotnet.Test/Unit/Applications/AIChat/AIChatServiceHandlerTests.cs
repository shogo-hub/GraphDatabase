using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Moq.Protected;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Backend.Dotnet.Application.AIChat.Configuration;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenAI;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenRouter;

// NOTE: Add other namespaces if you have more providers (e.g. Google, Anthropic)

namespace Backend.Dotnet.Tests.Unit.Applications.AIChat;

public class AIChatServiceHandlerTests
{
    private readonly IConfiguration _config;
    private readonly AIChatOptions _aiChatOptions;

    public AIChatServiceHandlerTests()
    {
        // 1. Locate and load the actual appsettings.json from the main project
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Backend/Dotnet"));
        
        var builder = new ConfigurationBuilder()
            .SetBasePath(projectPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables();

        _config = builder.Build();

        // 2. Bind the configuration to the Options object
        _aiChatOptions = new AIChatOptions();
        _config.GetSection("AIChat").Bind(_aiChatOptions);
    }

    [Fact]
    public async Task Handler_FromAppSettings_GeneratesCorrectRequestPayload()
    {
        // ARRANGE
        // 1. Determine which provider is active in the E2E settings
        var selectedProviderName = _config["AIChat:E2E:Provider"];

        if (string.IsNullOrEmpty(selectedProviderName) || selectedProviderName.Equals("Mock", StringComparison.OrdinalIgnoreCase))
        {
            // Skip this test if Mock is selected, as MockClient usually doesn't use HttpClient in the same way
            // or has trivial logic.
            return;
        }

        // 2. Retrieve the specific config for that provider (Model, BaseUrl, etc.)
        if (!_aiChatOptions.ProviderInfo.TryGetValue(selectedProviderName, out var providerConfig))
        {
            throw new InvalidOperationException($"Provider '{selectedProviderName}' is selected in settings but missing from 'ProviderInfo' section.");
        }

        // 3. Setup the Mock HttpClientHandler to capture the request
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                // Return a generic JSON valid for standard OpenAI-compatible parsers to prevent parsing errors
                Content = new StringContent("{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"Mock Response\"}}], \"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Mock Response\"}]}}]}")
            });

        // 4. Create HttpClient
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri(providerConfig.BaseUrl ?? "http://localhost")
        };

        // 5. Instantiate the specific Client class dynamically
        IAiClient client = CreateClientInstance(selectedProviderName, httpClient);

        // ACT
        var result = await client.QueryAsync("Make sure this prompt is in JSON");

        // ASSERT
        Assert.NotNull(capturedRequest);
        var jsonBody = await capturedRequest.Content.ReadAsStringAsync();

        // 1. Verify the exact Model defined in appsettings.json is used
        Assert.Contains(providerConfig.Model, jsonBody);

        // 2. Verify the prompt text is present
        Assert.Contains("Make sure this prompt is in JSON", jsonBody);

        // 3. Verify success result
        Assert.True(result.Success, $"Client failed to parse response: {result.Error}");
    }

    /// <summary>
    /// Helper to instantiate the correct client class based on the string name.
    /// This mimics logic that might be in DI or Factory, but for testing purposes.
    /// </summary>
    private IAiClient CreateClientInstance(string providerName, HttpClient httpClient)
    {
        var optionsWrapper = Options.Create(_aiChatOptions);

        return providerName.ToLowerInvariant() switch
        {
            "openai" => new OpenAiClient(httpClient, optionsWrapper, Mock.Of<ILogger<OpenAiClient>>()),
            "openrouter" => new OpenRouterClient(httpClient, optionsWrapper, Mock.Of<ILogger<OpenRouterClient>>()),
            // "gemini" => new GeminiClient(httpClient, optionsWrapper, ...), // Uncomment when implemented
            _ => throw new NotSupportedException($"The provider '{providerName}' is not supported in the test helper 'CreateClientInstance'. Add it to the switch case.")
        };
    }
}