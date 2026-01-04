using System.Security.Claims;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Represents an immutable authentication token that holds a collection of claims.
/// </summary>
/// <param name="claims">Initial set of claims contained in the token.</param>
public readonly struct Token(IEnumerable<Claim> claims)
{
    /// <summary>
    /// Initializes a new empty <see cref="Token"/> with no claims.
    /// </summary>
    public Token() : this(Enumerable.Empty<Claim>())
    {
    }

    /// <summary>
    /// Gets the claims contained in the token as a read-only collection.
    /// </summary>
    /// <remarks>
    /// The claims are copied to a list to ensure the token remains immutable.
    /// </remarks>
    public IReadOnlyCollection<Claim> Claims { get; } = claims.ToList();
}