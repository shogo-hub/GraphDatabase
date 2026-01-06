using Backend.Dotnet.Common.Miscellaneous;
using Backend.Dotnet.Common.Errors.Types;

namespace Backend.Dotnet.Application.AIChat;

/// <summary>
/// Service for AIChat (Retrieval-Augmented Generation) query orchestration.
/// Handles prompt template rendering, AI client calls, and result mapping.
/// </summary>
public interface IAIChatService
{
    /// <summary>
    /// Execute a AIChat query asynchronously.
    /// </summary>
    /// <param name="model">The domain model containing query, context, and task type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing either the AIChat result or an error.</returns>
    Task<TryResult<AIChatResultModel, Error>> QueryAsync(AIChatDomainModel model, CancellationToken cancellationToken = default);
}