namespace Backend.Dotnet.Common.Errors.Types;

/// <summary>
/// Error returned when an AI provider is not found or not configured.
/// </summary>
public sealed class ProviderNotFoundError : Error
{
    public ProviderNotFoundError(string detail, string providerName)
        : base("ai.provider_not_found", detail, new Params { ProviderName = providerName })
    {
    }

    public override string Title => "AI Provider Not Found";

    public override Params Parameters => (Params)base.Parameters!;

    public sealed class Params
    {
        public required string ProviderName { get; init; }
    }
}