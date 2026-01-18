using Backend.Dotnet.Common.Errors.Types;
using Backend.Dotnet.Common.Miscellaneous;
using Microsoft.Extensions.Logging;

namespace Backend.Dotnet.Application.AIChat.AIModelProvider;

/// <summary>
/// Factory for selecting the appropriate AI client based on provider name.
/// Supports dynamic provider selection from registered IAiClient implementations.
/// </summary>
public sealed class AIProviderFactory
{
    private readonly IEnumerable<IAiClient> _clients;
    private readonly ILogger<AIProviderFactory> _logger;

    /// <summary>
    /// Create a new <see cref="AIProviderFactory"/>.
    /// </summary>
    /// <param name="clients">All registered IAiClient implementations.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    public AIProviderFactory(
        IEnumerable<IAiClient> clients,
        ILogger<AIProviderFactory> logger)
    {
        _clients = clients;
        _logger = logger;
    }

    /// <summary>
    /// Get an AI client by provider name.
    /// Returns an error if the requested provider is not found.
    /// </summary>
    /// <param name="providerName">The name of the provider (e.g., "OpenAi", "Mock").</param>
    /// <returns>The matching IAiClient implementation.</returns>
    public TryResult<IAiClient, Error> GetClient(string providerName)
    {
        if (!_clients.Any())
        {
            return TryResult.Fail<Error>(new ProviderNotFoundError("No AI clients are registered", providerName));
        }

        if (string.IsNullOrWhiteSpace(providerName))
        {
            return TryResult.Fail<Error>(new ProviderNotFoundError("Provider name is empty", providerName));
        }
        // FirstOrDefault iterate Enum and return first one which meet the requirement.
        var client = _clients.FirstOrDefault(c => 
            c.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

        if (client == null)
        {
            _logger.LogWarning(
                "Provider {RequestedProvider} not found. Available providers: {AvailableProviders}",
                providerName,
                string.Join(", ", _clients.Select(c => c.ProviderName)));

            return TryResult.Fail<Error>(new ProviderNotFoundError("Requested AI provider was not found", providerName));
        }

        _logger.LogDebug("Selected AI provider: {Provider}", client.ProviderName);
        return TryResult.Succeed(client);
    }
}