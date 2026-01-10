namespace Backend.Dotnet.Common.Errors.Types;

/// <summary>
/// Error returned when an AI provider fails to generate a response (e.g. API error, timeout).
/// </summary>
public sealed class AiProviderError : Error
{
    public AiProviderError(string detail, string providerName, string? statusCode = null)
        : base("ai.provider_error", detail, new Params { ProviderName = providerName, StatusCode = statusCode })
    {
    }

    public override string Title => "AI Provider Error";

    public override Params Parameters => (Params)base.Parameters!;

    public sealed class Params
    {
        public required string ProviderName { get; init; }
        public string? StatusCode { get; init; }
    }
}