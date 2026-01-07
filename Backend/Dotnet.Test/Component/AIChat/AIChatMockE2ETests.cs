using System.Net;
using Backend.Dotnet.Controllers.AIChat.Models;
using Backend.Dotnet.Application.AIChat.PromptCreator;
using Backend.Dotnet.Tests.TestHelpers;
using Backend.Dotnet.Tests.TestHelpers.AIChat;
using Backend.Dotnet.Tests.TestHelpers.Http;
using IntegrationMocks.Core;

namespace Backend.Dotnet.Tests.Component.AIChat;

public sealed class AIChatMockE2ETests : IClassFixture<BackendServiceFixture>, IDisposable
{
    private readonly IInfrastructureService<BackendContract> _backend;
    private readonly BackendAIChatTestClient _aiChatClient;

    public AIChatMockE2ETests(BackendServiceFixture backendFixture)
    {
        _backend = backendFixture.Backend;
        _aiChatClient = new BackendAIChatTestClient(TestHttpClientFactory.Create(_backend.Contract.ApiUrl));
    }

    public void Dispose()
    {
        _aiChatClient.Dispose();
    }

    [Fact]
    public async Task Query_with_mock_provider_returns_mock_text()
    {
        var response = await _aiChatClient.PostQueryOkAsync(new AIChatQueryRequest
        {
            Query = "Explain what a graph database is.",
            Context = "",
            TaskType = PromptTemplateType.Explain,
            Provider = AiProvider.Mock
        });

        Assert.Contains("[MOCK]", response.Result);
        Assert.NotNull(response.Meta);
        Assert.Equal("Explain", response.Meta.TaskType);
        Assert.False(string.IsNullOrWhiteSpace(response.Meta.RequestId));
        Assert.True(response.Meta.DurationMs >= 0);
    }

    [Fact]
    public async Task Query_with_empty_query_returns_bad_request()
    {
        using var raw = await _aiChatClient.PostQueryRawAsync(new AIChatQueryRequest
        {
            Query = "",
            Context = "",
            TaskType = PromptTemplateType.Explain,
            Provider = AiProvider.Mock
        });

        Assert.Equal(HttpStatusCode.BadRequest, raw.StatusCode);
    }
}
