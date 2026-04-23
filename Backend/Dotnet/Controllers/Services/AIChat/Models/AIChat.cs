using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Backend.Dotnet.Application.AIChat.PromptCreator;

/*Model used with cliednt - server traffic*/

namespace Backend.Dotnet.Controllers.Service.AIChat.Models;

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
/// Request model for AIChat query endpoint.
/// </summary>
public sealed class AIChatQueryRequest
{
    [Required(ErrorMessage = "Query is required")]
    [StringLength(5000, MinimumLength = 1, ErrorMessage = "Query must be 1-5000 characters")]
    public required string Query { get; init; }

    // TODO : Add default hard coded context here for the demostration
    [StringLength(2000, ErrorMessage = "Context must be under 2000 characters")]
    public string? Context { get; init; }

    /// <summary>
    /// Type of prompt template to use. Default: Explain.
    /// </summary>
    [JsonPropertyName("taskType")]
    [EnumDataType(typeof(PromptTemplateType), ErrorMessage = "TaskType must be a valid PromptTemplateType")]
    public PromptTemplateType TaskType { get; init; } = PromptTemplateType.Explain;

    /// <summary>
    /// Select which AI provider to use for this request.
    /// Default: Mock. Server-side settings determine exact model mapping.
    /// </summary>
    [JsonPropertyName("provider")]
    [EnumDataType(typeof(AiProvider), ErrorMessage = "Provider must be one of: OpenAi, Mock, Test, OprnRouter")]
    public AiProvider Provider { get; init; } = AiProvider.Mock;
}

/// <summary>
/// Response model for AIChat query endpoint.
/// </summary>
public sealed class AIChatQueryResponse
{
    public required string Result { get; init; }
    public required AIChatMetadata Meta { get; init; }
}

/// <summary>
/// Metadata about the AIChat query execution.
/// </summary>
public sealed class AIChatMetadata
{
    public required long DurationMs { get; init; }
    public required string TaskType { get; init; }
    public required string RequestId { get; init; }
}