using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Backend.Dotnet.Common.Serialization.Json;
using Backend.Dotnet.Controllers.Service.AIChat.Models;
using Backend.Dotnet.Tests.TestHelpers;
using Backend.Dotnet.Tests.TestHelpers.Http;
using Xunit;

namespace Backend.Dotnet.Tests.Component.AIChat;

public sealed class AIChatControllerTests : IClassFixture<BackendServiceFixture>
{
    private readonly TestHttpClient _client;

    public AIChatControllerTests(BackendServiceFixture fixture)
    {
        _client = TestHttpClientFactory.Create(fixture.Backend.Contract.ApiUrl);
    }

    [Fact]
    public async Task PostQuery_ReturnsBadRequest_WhenQueryIsEmpty()
    {
        // ARRANGE
        var request = new AIChatQueryRequest
        {
            Query = "",
            Provider = AiProvider.Mock
        };

        // ACT
        using var response = await _client.PostAsJsonAsync(
            "api/v1/AIChat/query",
            request,
            ControllerApiJsonSerializer.Options);

        var body = await response.Content.ReadAsStringAsync();

        // ASSERT
        Assert.True(
            response.StatusCode == HttpStatusCode.BadRequest,
            $"Expected 400 BadRequest. Got {(int)response.StatusCode} {response.StatusCode}. Body={body}");
    }

    [Fact]
    public async Task PostQuery_ReturnsOk_WhenProviderIsMock()
    {
        // ARRANGE
        var request = new AIChatQueryRequest
        {
            Query = "Hello",
            Provider = AiProvider.Mock
        };

        // ACT
        using var response = await _client.PostAsJsonAsync(
            "api/v1/AIChat/query",
            request,
            ControllerApiJsonSerializer.Options);

        var body = await response.Content.ReadAsStringAsync();

        // ASSERT
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected 200 OK. Got {(int)response.StatusCode} {response.StatusCode}. Body={body}");

        var payload = await response.Content.ReadFromJsonAsync<AIChatQueryResponse>(ControllerApiJsonSerializer.Options);
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Result), "Result should not be empty.");

        // Mock の場合は決定的な文字列が含まれる想定（Mock実装に合わせて調整）
        Assert.Contains("[MOCK:", payload.Result);
    }
}