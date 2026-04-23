using Backend.Dotnet.Application.AIChat.Configuration;
using Backend.Dotnet.Common.Errors.Types;
using Backend.Dotnet.Common.Miscellaneous;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Backend.Dotnet.Application.AIChat.AIModelProvider.Mock;

/// <summary>
/// Mock implementation of IAiClient for testing without calling external AI services.
/// Returns deterministic responses based on prompt content.
/// </summary>
public sealed class MockTestClient : IAiClient
{
    private readonly ILogger<MockTestClient> _logger;
    private readonly AiProviderOptions _options;

    public string ProviderName => "Mock";

    /// <summary>
    /// Create a new <see cref="MockTestClient"/>.
    /// </summary>
    /// <param name="options">Configuration options containing Mock provider settings.</param>
    /// <param name="logger">Logger for telemetry.</param>
    public MockTestClient(
        IOptions<AIChatOptions> options, 
        ILogger<MockTestClient> logger)
    {
        _logger = logger;
        // Ensure "Mock" entry exists in appsettings.json or throws exception, just like real providers.
        _options = options.Value.GetRequiredProvider("Mock");
    }

    /// <summary>
    /// Return a prepared mock response without calling external AI services.
    /// Behaviour:
    /// - Throws <see cref="ArgumentException"/> if <paramref name="prompt"/> is null or whitespace.
    /// - Returns deterministic mock text based on prompt length and content.
    /// - Honors <paramref name="cancellationToken"/> for consistency with IAiClient contract.
    /// - Logs request id and response details for testing observability.
    /// </summary>
    /// <param name="prompt">Fully rendered prompt (template + domain data).</param>
    /// <param name="cancellationToken">Cancellation token (for interface compatibility).</param>
    /// <returns>Mock assistant text. Never null.</returns>
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
            _logger.LogDebug(
                "Mock AI request started. RequestId={RequestId}, PromptLength={PromptLength}, Model={Model}",
                requestId, prompt.Length, _options.Model);
            
            // Simulate network delay
            await Task.Delay(100, cancellationToken);

            // Use the shared method for deterministic response generation
            var response = GenerateMockResponse(prompt);

            var duration = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            _logger.LogInformation(
                "Mock AI request completed. RequestId={RequestId}, DurationMs={DurationMs}, ResponseLength={ResponseLength}",
                requestId, duration, response.Length);

            return TryResult.Succeed(response);
        }
        catch (Exception ex)
        {
            var duration = (DateTimeOffset.UtcNow - start).TotalMilliseconds;

            return TryResult.Fail<Error>(new AiProviderError(
                $"Exception during Mock request: {ex.Message}",
                ProviderName));
        }
    }

    /// <summary>
    /// Generate a deterministic mock response based on prompt content.
    /// Can be extended to return different responses for different task types.
    /// </summary>
    private string GenerateMockResponse(string prompt)
    {
        // Simple deterministic response based on prompt characteristics
        var promptLower = prompt.ToLowerInvariant();
        var modelTag = $"[MOCK:{_options.Model ?? "default"}]";

        if (promptLower.Contains("summarize"))
        {
            return $"{modelTag} This is a mock summary. The content has been analyzed and condensed into key points for demonstration purposes.";
        }

        if (promptLower.Contains("explain"))
        {
            return $"{modelTag} This is a mock explanation. The concept has been broken down into understandable parts with examples for testing purposes.";
        }

        // Default answer response
        return $"{modelTag} This is a mock answer response. Prompt length: {prompt.Length} characters. This response is generated for testing without calling external AI services.";
    }
}