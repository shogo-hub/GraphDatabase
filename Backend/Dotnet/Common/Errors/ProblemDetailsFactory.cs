using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using Backend.Dotnet.Common.Errors.Types;

namespace Backend.Common.Errors;

internal sealed class ProblemDetailsFactory(IOptions<ErrorsOptions> options) : IProblemDetailsFactory
{
    private readonly IOptions<ErrorsOptions> _options = options;

    /// <summary>
    /// Creates a <see cref="ProblemDetails"/> instance that represents the provided domain <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The domain error to convert into a problem details payload.</param>
    /// <param name="pathEntityTypes">A mapping from path identifiers to entity type names used to
    /// disambiguate entity-not-found errors for correct HTTP status selection.</param>
    /// <returns>A populated <see cref="ProblemDetails"/> instance with <see cref="ProblemDetails.Status"/>,
    /// <see cref="ProblemDetails.Title"/>, <see cref="ProblemDetails.Detail"/> and additional extensions.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided <paramref name="error"/> type is not supported.</exception>
    public ProblemDetails Create(Error error, IReadOnlyDictionary<string, string> pathEntityTypes)
    {
        var status = error switch
        {
            AuthenticationFailedError => HttpStatusCode.Unauthorized,
            AuthorizationFailedError => HttpStatusCode.Forbidden,
            EntityNotFoundError entityNotFoundError =>
                pathEntityTypes.GetValueOrDefault(entityNotFoundError.Parameters.EntityId)
                == entityNotFoundError.Parameters.EntityType
                ? HttpStatusCode.NotFound
                : HttpStatusCode.BadRequest,
            ValidationFailedError => HttpStatusCode.BadRequest,
            _ => throw new InvalidOperationException($"{error.GetType} is not supported.")
        };

        return new ProblemDetails
        {
            Detail = error.Detail,
            Status = (int)status,
            Title = error.Title,
            Type = new Uri(_options.Value.Url, $"{(int)status}/{error.Code}").ToString(),
            Extensions = new Dictionary<string, object?>
            {
                ["parameters"] = error.Parameters
            }
        };
    }
}