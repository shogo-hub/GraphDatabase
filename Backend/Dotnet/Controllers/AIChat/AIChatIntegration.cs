using Backend.Application.AIChat;
using Backend.Common.Errors;
using Backend.Common.Errors.AspNetCore;
using Backend.WebApi.RagIntegration.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.WebApi.RagIntegration;

[ApiController]
[Route("api/v1/rag")]
[Produces("application/json")]
public sealed class AIChatController : ControllerBase
{
    private readonly IAIChatService _AIChatService;
    private readonly ILogger<AIChatController> _logger;
    private readonly IProblemDetailsFactory _problemDetailsFactory;

    ///<summary>
    /// Constructor
    ///</summary>
    public AIChatController(
        IAIChatService ragService,
        ILogger<AIChatController> logger,
        IProblemDetailsFactory problemDetailsFactory)
    {
        _AIChatService = ragService;
        _logger = logger;
        _problemDetailsFactory = problemDetailsFactory;
    }

    /// <summary>
    /// Process a RAG (Retrieval-Augmented Generation) query by rendering a prompt template,
    /// calling the AI service, and returning the generated response with metadata.
    /// </summary>
    /// <param name="request">The RAG query request containing the user's question, optional context, and task type.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// 200 OK with the AI-generated result and metadata (duration, task type, request ID) on success.
    /// 400 Bad Request if validation fails.
    /// 503 Service Unavailable if the AI service is unreachable.
    /// 500 Internal Server Error for unexpected failures.
    /// </returns>
    /// <remarks>
    /// The method validates the input using DataAnnotations, maps the request to a domain model,
    /// calls <see cref="IRagService.QueryAsync"/> which renders the prompt template and queries the AI provider,
    /// then returns the result with timing metadata.
    /// All requests are logged with a unique request ID for traceability.
    /// </remarks>
    [HttpPost("query")]
    public async Task<IActionResult> QueryAsync(
        [FromBody] RagQueryRequest request,
        CancellationToken ct)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var start = DateTimeOffset.UtcNow;


        // Validate input (ModelState validates DataAnnotations automatically)
        if (!ModelState.IsValid)
        {
            _logger.LogWarning(
                "Validation failed. RequestId={RequestId}, Errors={Errors}",
                requestId, ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return BadRequest(new { error = "validation_failed", details = ModelState });
        }

        // Map to domain model
        var domainModel = AIChatMapper.ToDomain(request);

        // Call service
        var result = await _AIChatService.QueryAsync(domainModel, ct);

        // Check if the operation succeeded
        if (!result.IsSucceeded)
        {
            var problemDetails = _problemDetailsFactory.Create(result.Error!);
            return problemDetails.ToActionResult();
        }

        var duration = (long)(DateTimeOffset.UtcNow - start).TotalMilliseconds;

        return Ok(new RagQueryResponse
        {
            Result = result.Value.Output,
            Meta = new RagMetadata
            {
                DurationMs = duration,
                TaskType = domainModel.TaskType,
                RequestId = requestId
            }
        });

    }
}