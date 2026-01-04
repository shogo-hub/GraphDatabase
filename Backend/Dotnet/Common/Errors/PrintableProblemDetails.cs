using Backend.Common.Serialization.Json;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Backend.Dotnet.Common.Errors;

/// <summary>
/// A <see cref="ProblemDetails"/> that provides a JSON representation when converted to string.
/// </summary>
/// <remarks>
/// Useful for logging or debugging where a compact JSON representation of the problem
/// details is desirable. Uses the project's <see cref="WebApiJsonSerializer.Options"/> settings.
/// </remarks>
public sealed class PrintableProblemDetails : ProblemDetails
{
    /// <summary>
    /// Serializes the current <see cref="ProblemDetails"/> instance to a JSON string
    /// using the configured serializer options.
    /// </summary>
    /// <returns>A JSON string representing the problem details.</returns>
    public override string ToString()
    {
        return JsonSerializer.Serialize(this, WebApiJsonSerializer.Options);
    }
}