namespace Backend.Common.Errors;

/// <summary>
/// Options for configuring error handling behavior in ASP.NET Core integration.
/// </summary>
public sealed class ErrorsOptions
{
    /// <summary>
    /// The base URL that identifies the error registry or documentation endpoint.
    /// Clients can use this URI to find more information about error codes.
    /// </summary>
    public required Uri Url { get; init; }
}