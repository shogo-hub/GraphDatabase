using System.Text;
using System.Text.Json;
using Backend.Dotnet.Application.AIChat.Configuration;
using Backend.Dotnet.Common.Errors.Types;
using Backend.Dotnet.Common.Miscellaneous;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Dotnet.Application.AIChat.AIModelProvider.OpenAI;

public sealed class OpenAiClient : IAiClient
{
    private readonly HttpClient _http;
    private readonly AiProviderOptions _options;
    private readonly ILogger<OpenAiClient> _logger;

    public string ProviderName => "OpenAi";

    /// <summary>
    /// Create a new <see cref="OpenAiClient"/>.
    /// </summary>
    /// <param name="httpClient">Typed HttpClient configured for the AI provider.</param>
    /// <param name="options">Bound AI settings from configuration.</param>
    /// <param name="logger">Logger for telemetry and errors.</param>
    public OpenAiClient(
        HttpClient httpClient,
        IOptions<AIChatOptions> options,
        ILogger<OpenAiClient> logger)
    {
        _http = httpClient;
        _options = options.Value.GetRequiredProvider("OpenAi");
        _logger = logger;
    }

    /// <summary>
    /// Send the rendered prompt to the configured AI model and return the generated text.
    /// Behaviour:
    /// - Throws <see cref="ArgumentException"/> if <paramref name="prompt"/> is null or whitespace.
    /// - Sends a POST to "v1/chat/completions" (payload uses <see cref="AiOptions.Model"/> and <see cref="AiOptions.MaxTokens"/>).
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
                "Sending AI request. RequestId={RequestId}, Model={Model}, PromptLength={PromptLength}",
                requestId, _options.Model, prompt.Length);

            using var response = await _http.SendAsync(request, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {                
                return TryResult.Fail<Error>(new AiProviderError(
                    $"AI API returned {response.StatusCode}: {responseBody}",
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
                "AI request completed. RequestId={RequestId}, DurationMs={DurationMs}, ResponseLength={ResponseLength}",
                requestId, duration, content?.Length ?? 0);

            return TryResult.Succeed(content ?? string.Empty);
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            var duration = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            return TryResult.Fail<Error>(new AiProviderError(
                $"Exception during AI request: {ex.Message}",
                ProviderName));
        }
    }
}