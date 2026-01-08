using System.Net;
using Microsoft.Extensions.Configuration;
using Xunit;
using Xunit.Sdk;
using Backend.Dotnet.Tests.TestHelpers;
using Backend.Dotnet.Tests.TestHelpers.AIChat;

namespace Backend.Dotnet.Tests.Component.AIChat;

public sealed class AIChatE2EFromAppSettingsTests : IClassFixture<BackendServiceFixture>
{
    private readonly BackendServiceFixture _fixture;

    public AIChatE2EFromAppSettingsTests(BackendServiceFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Query_uses_provider_from_appsettings_and_returns_answer()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var provider = config["AIChat:E2E:Provider"] ?? "Mock";

        // 1. Ensure configuration exists for the selected provider (Mock included)
        var providerSection = config.GetSection($"AIChat:ProviderInfo:{provider}");
        if (!providerSection.Exists())
        {
             throw new InvalidOperationException($"Provider '{provider}' is selected for E2E but configuration 'AIChat:ProviderInfo:{provider}' is missing.");
        }

        // 2. Validate ApiKey configuration for ALL providers 
        var apiKey = providerSection["ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
             throw new InvalidOperationException(
                 $"ApiKey is missing for provider '{provider}'. " +
                 $"Please configure 'AIChat:ProviderInfo:{provider}:ApiKey' in appsettings or environment variables.");
        }

        using var http = new HttpClient { BaseAddress = _fixture.Backend.Contract.ApiUrl };
        using var client = new BackendAIChatTestClient(http);

        // NOTE: property names must match your controller's request model JSON names
        var request = new
        {
            query = "Explain what a graph database is in one short sentence.",
            provider = provider,
            taskType = "Explain"
        };

        using var response = await client.PostQueryRawAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK. Got {(int)response.StatusCode} {response.StatusCode}. Body={body}");

        // Non-mock: don't check accuracy, just verify something was generated
        Assert.False(string.IsNullOrWhiteSpace(body));

        // Mock: strong deterministic assertion (adjust if your mock output differs)
        if (provider.Equals("Mock", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Contains("[MOCK]", body);
        }
    }
}
