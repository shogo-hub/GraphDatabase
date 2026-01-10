namespace Backend.Dotnet.Application.AIChat.Configuration;

public sealed class AIChatOptions
{
    public E2EOptions E2E { get; init; } = new();

    /// <summary>
    /// ProviderName -> connection/model settings.
    /// Keys are treated case-insensitively.
    /// </summary>
    public Dictionary<string, AiProviderOptions> ProviderInfo { get; init; } =
        new(StringComparer.OrdinalIgnoreCase);

    public bool TryGetProvider(string providerName, out AiProviderOptions options)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            options = default!;
            return false;
        }

        return ProviderInfo.TryGetValue(providerName, out options!);
    }

    public AiProviderOptions GetRequiredProvider(string providerName)
    {
        if (TryGetProvider(providerName, out var options))
        {
            return options;
        }

        throw new InvalidOperationException($"Missing configuration for provider '{providerName}' under AIChat:ProviderInfo.");
    }
}

public sealed class E2EOptions
{
    public string Provider { get; init; } = "Mock";
}

public sealed class AiProviderOptions
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public required string Model { get; init; }
    public required int MaxTokens { get; init; }
    public required int TimeoutSeconds { get; init; }
}
