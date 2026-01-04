using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

/*Model used with cliednt - server traffic*/

namespace Backend.WebApi.RagIntegration.Models;

// Enum for choosing model platform(or mock test)
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AiProvider
{
    OpenAi,
    Mock,
    Test,
    OpenRouter
}

/// <summary>
/// Request model for RAG query endpoint.
/// </summary>
public sealed class RagQueryRequest
{
    [Required(ErrorMessage = "Query is required")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Query must be 1-5000 characters")]
    public required string Query { get; init; }

    // TODO : Add default hard coded context here for the demostration
    [StringLength(2000, ErrorMessage = "Context must be under 2000 characters")]
    public string? Context { get; init; }

    [RegularExpression("^(answer|summarize|explain)$", ErrorMessage = "TaskType must be 'answer', 'summarize', or 'explain'")]
    public string TaskType { get; init; } = "answer";

    /// <summary>
    /// Select which AI provider to use for this request.
    /// Default: Mock. Server-side settings determine exact model mapping.
    /// </summary>
    [JsonPropertyName("provider")]
    [EnumDataType(typeof(AiProvider), ErrorMessage = "Provider must be one of: OpenAi, Mock, Test, OprnRouter")]
    public AiProvider Provider { get; init; } = AiProvider.Mock;
}

/// <summary>
/// Response model for RAG query endpoint.
/// </summary>
public sealed class RagQueryResponse
{
    public required string Result { get; init; }
    public required RagMetadata Meta { get; init; }
}

/// <summary>
/// Metadata about the RAG query execution.
/// </summary>
public sealed class RagMetadata
{
    public required long DurationMs { get; init; }
    public required string TaskType { get; init; }
    public required string RequestId { get; init; }
}