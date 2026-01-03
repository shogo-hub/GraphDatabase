using Backend.Common.Errors;
using Backend.Common.Miscellaneous;

namespace Backend.Application.AIChat.AIModelProvider;

/// <summary>
/// Interface for managing AI request/response.
/// Handle query for AI asynchronously.
/// </summary>
internal interface IAiClient
{
    /// <summary>
    /// The name of this AI provider (e.g., "OpenAi", "Mock").
    /// Used by the factory to select the appropriate implementation.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Send a prompt to the AI provider and return the generated response.
    /// </summary>
    /// <param name="prompt">The fully rendered prompt to send to the AI.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request.</param>
    /// <returns>The AI-generated response text or an error.</returns>
    Task<TryResult<string, Error>> QueryAsync(string prompt, CancellationToken cancellationToken = default);
}