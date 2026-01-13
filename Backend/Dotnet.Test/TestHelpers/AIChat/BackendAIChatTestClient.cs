using System.Text;
using System.Text.Json;
using Backend.Dotnet.Common.Serialization.Json;
using Backend.Dotnet.Controllers.Service.AIChat.Models;

namespace Backend.Dotnet.Tests.TestHelpers.AIChat;

internal sealed class BackendAIChatTestClient(HttpClient inner) : IDisposable
{
    public const string QueryPath = "/api/v1/AIChat/query";

    private readonly HttpClient _inner = inner;

    public void Dispose()
    {
        _inner.Dispose();
    }

    public async Task<HttpResponseMessage> PostQueryRawAsync(
        AIChatQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, ControllerApiJsonSerializer.Options);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _inner.PostAsync(QueryPath, content, cancellationToken);
    }

    public async Task<HttpResponseMessage> PostQueryRawAsync(
        object request,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(request, ControllerApiJsonSerializer.Options);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _inner.PostAsync(QueryPath, content, cancellationToken);
    }

    public async Task<AIChatQueryResponse> PostQueryOkAsync(
        AIChatQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostQueryRawAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"AIChat query failed: {(int)response.StatusCode} {response.StatusCode}. Body={body}");
        }

        return JsonSerializer.Deserialize<AIChatQueryResponse>(body, ControllerApiJsonSerializer.Options)
            ?? throw new InvalidOperationException("AIChat response deserialized to null.");
    }
}
