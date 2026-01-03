using Backend.Common.Miscellaneous;
using Backend.Common.Errors;

namespace Backend.Application.Rag;

/// <summary>
/// Service for RAG (Retrieval-Augmented Generation) query orchestration.
/// Handles prompt template rendering, AI client calls, and result mapping.
/// </summary>
public interface IRagService
{
    /// <summary>
    /// Execute a RAG query asynchronously.
    /// </summary>
    /// <param name="model">The domain model containing query, context, and task type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A result containing either the RAG result or an error.</returns>
    Task<TryResult<RagResultModel, Error>> QueryAsync(RagDomainModel model, CancellationToken cancellationToken = default);
}