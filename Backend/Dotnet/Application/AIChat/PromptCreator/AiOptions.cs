namespace Backend.Dotnet.Application.AIChat.PromptCreator;

public sealed class AiOptions
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string Model { get; init; }
    public required int MaxTokens { get; init; }
    public required int TimeoutSeconds { get; init; }
}