using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Backend.Dotnet.Application.AIChat.AIModelProvider;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenAI;
using Backend.Dotnet.Application.AIChat.AIModelProvider.OpenRouter;
using Backend.Dotnet.Application.AIChat.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Dotnet.Tests.Unit.Applications.AIChat;

public sealed class AIModelProviderClientsTests
{
    private readonly AIChatOptions _loadedOptions;
    private readonly IConfiguration _configuration;

    public AIModelProviderClientsTests()
    {
        // Resolve path to appsettings.json.
        // Assuming test execution in bin/Debug/net9.0, we traverse up to the project root or use a relative path.
        // Adjust the "../../../../" part based on your actual folder structure depth.
        // Workspace: /home/shogo/myproduct/GraphDatabase
        // AppSettings: Backend/Dotnet/appsettings.json
        var workspaceRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../"));
        var configPath = Path.Combine(workspaceRoot, "Backend/Dotnet/appsettings.json");

        if (!File.Exists(configPath))
        {
            // Fallback for CI or other environments where path might differ
            configPath = "appsettings.json"; 
        }

        _configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: true)
            .Build();

        _loadedOptions = new AIChatOptions();
        _configuration.GetSection("AIChat").Bind(_loadedOptions);
    }

    [Theory]
    [InlineData("OpenRouter")]
    [InlineData("OpenAi")]
    public async Task AiClient_HybridTest_RunsRealApiIfKeyExists_ElseRunsMock(string providerName)
    {
        // ARRANGE
        if (!_loadedOptions.ProviderInfo.TryGetValue(providerName, out var providerOptions))
        {
            // Fallback options if not found in appsettings
            providerOptions = new AiProviderOptions 
            { 
                BaseUrl = "http://mock-fallback", 
                ApiKey = "", 
                Model = "mock-model", 
                MaxTokens = 100, 
                TimeoutSeconds = 10 
            };
        }

        // Check if IsMock is enabled in appsettings or ApiKey is missing
        bool isMockExplicit = _configuration.GetValue<bool>($"AIChat:ProviderInfo:{providerName}:IsMock");
        bool hasApiKey = !string.IsNullOrWhiteSpace(providerOptions.ApiKey);
        bool useMock = isMockExplicit || !hasApiKey;

        HttpClient httpClient;

        if (!useMock)
        {
            // [REAL MODE] Use real HttpClient
            httpClient = new HttpClient
            {
                BaseAddress = new Uri(providerOptions.BaseUrl),
                Timeout = TimeSpan.FromSeconds(providerOptions.TimeoutSeconds)
            };
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {providerOptions.ApiKey}");
            
            if (providerName == "OpenRouter")
            {
                httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:test");
                httpClient.DefaultRequestHeaders.Add("X-Title", "GraphDatabase-Test");
            }
        }
        else
        {
            // [MOCK MODE] Use Mock HttpHandler
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"[MOCK RESPONSE] AI is working.\"}}]}",
                        Encoding.UTF8,
                        "application/json")
                });

            httpClient = new HttpClient(mockHandler.Object)
            {
                BaseAddress = new Uri("http://mock-url/")
            };
        }

        // Ensure provider data exists in options for the client
        if (!_loadedOptions.ProviderInfo.ContainsKey(providerName))
        {
            _loadedOptions.ProviderInfo[providerName] = providerOptions;
        }
        var optionsWrapper = Options.Create(_loadedOptions);

        IAiClient client;
        if (providerName.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
        {
            client = new OpenRouterClient(httpClient, optionsWrapper, Mock.Of<ILogger<OpenRouterClient>>());
        }
        else if (providerName.Equals("OpenAi", StringComparison.OrdinalIgnoreCase))
        {
            client = new OpenAiClient(httpClient, optionsWrapper, Mock.Of<ILogger<OpenAiClient>>());
        }
        else
        {
            throw new NotSupportedException($"Test setup not implemented for {providerName}");
        }

        // ACT
        var prompt = "Hello! Please say 'Test Successful'.";
        var result = await client.QueryAsync(prompt);

        // ASSERT
        Assert.True(result.IsSucceeded, $"Request failed for {providerName}. Error: {result.Error?.Detail}");
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);

        if (!useMock)
        {
            // If real API, we expect a real response
            Assert.DoesNotContain("[MOCK RESPONSE]", result.Value);
        }
        else
        {
            // If mock, we expect our mock string
            Assert.Contains("[MOCK RESPONSE]", result.Value);
        }
    }
}
