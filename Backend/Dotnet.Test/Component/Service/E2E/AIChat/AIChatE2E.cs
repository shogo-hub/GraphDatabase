// using System.Net;
// using System.Net.Http.Json;
// using Backend.Dotnet.Application.AIChat.PromptCreator;
// using Backend.Dotnet.Common.Serialization.Json;
// using Backend.Dotnet.Controllers.Service.AIChat.Models;
// using Microsoft.Extensions.Configuration;
// using Xunit;
// using Backend.Dotnet.Tests.TestHelpers.Http;
// using Backend.Dotnet.Tests.TestHelpers;

// namespace Backend.Dotnet.Tests.Component.AIChat;

// public sealed class AIChatE2EFromAppSettingsTests : IClassFixture<BackendServiceFixture>
// {
//     private readonly BackendServiceFixture _fixture;
//     private readonly TestHttpClient _client;

//     public AIChatE2EFromAppSettingsTests(BackendServiceFixture fixture)
//     {
//         _fixture = fixture;
//         _client = TestHttpClientFactory.Create(fixture.Backend.Contract.ApiUrl);
//     }

//     [Fact]
//     public async Task Query_uses_provider_from_appsettings_and_returns_answer()
//     {

//         // ARRANGE
//         var config = new ConfigurationBuilder()
//             .AddJsonFile("appsettings.json", optional: true)
//             .AddEnvironmentVariables()
//             .Build();

//         var provider = config["AIChat:E2E:Provider"] ?? "Mock";
//         if (!Enum.TryParse<AiProvider>(provider, ignoreCase: true, out var providerEnum))
//         {
//             throw new InvalidOperationException(
//                 $"Provider '{provider}' is invalid. Expected one of: {string.Join(", ", Enum.GetNames<AiProvider>())}.");
//         }

//         // 1. Ensure configuration exists for the selected provider (Mock included)
//         var providerSection = config.GetSection($"AIChat:ProviderInfo:{provider}");
//         if (!providerSection.Exists())
//         {
//              throw new InvalidOperationException($"Provider '{provider}' is selected for E2E but configuration 'AIChat:ProviderInfo:{provider}' is missing.");
//         }

//         // 2. Validate ApiKey only for non-mock providers.
//         if (providerEnum != AiProvider.Mock)
//         {
//             var apiKey = providerSection["ApiKey"];
//             if (string.IsNullOrWhiteSpace(apiKey))
//             {
//                 throw new InvalidOperationException(
//                     $"ApiKey is missing for provider '{provider}'. " +
//                     $"Please configure 'AIChat:ProviderInfo:{provider}:ApiKey' in appsettings or environment variables.");
//             }
//         }

//         var request = new AIChatQueryRequest
//         {
//             Query = "Explain what a graph database is in one short sentence.",
//             Provider = providerEnum,
//             TaskType = PromptTemplateType.Explain
//         };

//         using var response = await _client.PostAsJsonAsync(
//             "/api/v1/AIChat/query",
//             request,
//             ControllerApiJsonSerializer.Options);
            
//         var body = await response.Content.ReadAsStringAsync();
        
//         // ASSERT
//         Assert.True(
//             response.StatusCode == HttpStatusCode.OK,
//             $"Expected 200 OK. Got {(int)response.StatusCode} {response.StatusCode}. Body={body}");

//         if (providerEnum == AiProvider.Mock)
//         {
//             // Mock: strong deterministic assertion (adjust if your mock output differs)
//             Assert.Contains("[MOCK]", body);
//         }
//         else
//         {
//             // Non-mock: don't check accuracy, just verify something was generated
//             Assert.False(string.IsNullOrWhiteSpace(body), "Response body should not be empty or whitespace.");
//         }
//     }
// }
