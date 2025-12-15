using Microsoft.AspNetCore.Mvc;
using Backend.Common.Errors.Types;
namespace Backend.Common.Errors;

/// <summary>
/// Factory interface for creating <see cref="ProblemDetails"/> instances from
/// domain <see cref="Error"/> models.
/// </summary>
public interface IProblemDetailsFactory
{
    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> representation for the provided
    /// <paramref name="error"/>, optionally enriching it with information about
    /// related entity types for paths.
    /// </summary>
    /// <param name="error">The domain error to convert to a <see cref="ProblemDetails"/>.</param>
    /// <param name="pathEntityTypes">A mapping from JSON path (or property path) to entity type names
    /// used to help clients interpret validation or error paths.</param>
    /// <returns>A populated <see cref="ProblemDetails"/> instance representing the error.</returns>
    ProblemDetails Create(Error error, IReadOnlyDictionary<string, string> pathEntityTypes);
}