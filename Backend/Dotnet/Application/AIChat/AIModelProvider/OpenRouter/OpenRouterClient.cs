using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Backend.Common.Errors;
using Backend.Common.Miscellaneous;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Backend.Application.Rag.PromptCreator;

namespace Backend.Application.Rag.AIModelProvider.OpenRouter;

/// <summary>
/// OpenRouter AI client implementation for testing.
/// Uses free models from OpenRouter (OpenAI-compatible API).
/// </summary>
public sealed class TestClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly AiOptions  _options;
    private readonly ILogger<TestClient> _logger;

    public string ProviderName => "OpenRouter";

    /// <summary>
    /// Create a new <see cref="TestClient"/>.
    /// </summary>
    /// <param name="httpClient">Typed HttpClient configured for OpenRouter.</param>
    /// <param name="options">Bound Test settings from configuration.</param>
    /// <param name="logger">Logger for telemetry and errors.</param>
    public TestClient(
        HttpClient httpClient,
        IOptions<AiOptions > options,
        ILogger<TestClient> logger)
    {
        _http = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Send the rendered prompt to OpenRouter's free AI models and return the generated text.
    /// Behaviour:
    /// - Throws <see cref="ArgumentException"/> if <paramref name="prompt"/> is null or whitespace.
    /// - Sends a POST to "v1/chat/completions" (OpenAI-compatible endpoint).
    /// - Honors <paramref name="cancellationToken"/> so callers can cancel long-running requests.
    /// - Returns <see cref="AiProviderError"/> for non-success HTTP responses.
    /// - Logs request id, durations and response length but avoids logging prompt content or API keys.
    /// </summary>
    /// <param name="prompt">Fully rendered prompt (template + domain data).</param>
    /// <param name="cancellationToken">Cancellation token to cancel the HTTP call.</param>
    /// <returns>The assistant text produced by the model or an error.</returns>
    public async Task<TryResult<string, Error>> QueryAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
        {
            throw new ArgumentException("Prompt cannot be empty", nameof(prompt));
        }

        var requestId = Guid.NewGuid().ToString("N")[..8];
        var start = DateTimeOffset.UtcNow;
        // Define pay load for OpenAI API
        try
        {
            var payload = new
            {
                model = _options.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = _options.MaxTokens
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            _logger.LogDebug(
                "Sending OpenRouter (Test) request. RequestId={RequestId}, Model={Model}, PromptLength={PromptLength}",
                requestId, _options.Model, prompt.Length);

            using var response = await _http.SendAsync(request, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {

                return TryResult.Fail<Error>(new AiProviderError(
                    $"OpenRouter API returned {response.StatusCode}: {responseBody}",
                    ProviderName,
                    response.StatusCode.ToString()));
            }

            using var doc = JsonDocument.Parse(responseBody);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            var duration = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            _logger.LogInformation(
                "OpenRouter (Test) request completed. RequestId={RequestId}, DurationMs={DurationMs}, ResponseLength={ResponseLength}",
                requestId, duration, content?.Length ?? 0);

            return TryResult.Succeed(content ?? string.Empty);
        }
        catch (Exception ex) when (ex is not ArgumentException) // ArgumentException is a programming error, let it bubble up? Or return error? The original code threw it.
        {
            var duration = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            return TryResult.Fail<Error>(new AiProviderError(
                $"Exception during OpenRouter request: {ex.Message}",
                ProviderName));
        }
    }
}