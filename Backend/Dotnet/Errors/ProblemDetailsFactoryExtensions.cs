using Microsoft.AspNetCore.Mvc;
using System.Collections.Immutable;
using Backend.Common.Errors.Types;

namespace Backend.Common.Errors;

/// <summary>
/// Extension helpers for <see cref="IProblemDetailsFactory"/>.
/// </summary>
public static class ProblemDetailsFactoryExtensions
{
    /// <summary>
    /// Convenience overload that creates a <see cref="ProblemDetails"/> from the provided
    /// <paramref name="error"/> without any path-to-entity-type mappings.
    /// </summary>
    /// <param name="self">The factory instance to invoke.</param>
    /// <param name="error">The domain error to convert to <see cref="ProblemDetails"/>.</param>
    /// <returns>A <see cref="ProblemDetails"/> representation of the error.</returns>
    public static ProblemDetails Create(this IProblemDetailsFactory self, Error error)
    {
        return self.Create(error, ImmutableDictionary<string, string>.Empty);
    }
}