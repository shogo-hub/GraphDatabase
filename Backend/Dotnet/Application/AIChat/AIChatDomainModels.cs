using Backend.Dotnet.Application.AIChat.PromptCreator;

namespace Backend.Dotnet.Application.AIChat;

/// <summary>
/// Domain model representing a AIChat query request.
/// </summary>
public sealed record AIChatDomainModel
{
    public required string Query { get; init; }
    public string? Context { get; init; }
    public PromptTemplateType TaskType { get; init; } = PromptTemplateType.Explain;
    public required string Provider { get; init; }
}

/// <summary>
/// Domain model representing a AIChat query result.
/// </summary>
public sealed record AIChatResultModel
{
    public required string Output { get; init; }
}