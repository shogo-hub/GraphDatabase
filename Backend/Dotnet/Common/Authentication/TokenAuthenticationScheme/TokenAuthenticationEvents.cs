namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
///  Holds callback delegates that the authentication handler invokes at lifecycle points.
/// </summary>
public class TokenAuthenticationEvents
{
    /// <summary>
    /// Delegate(The variable which lambda function is assigned) with setter/getter
    /// when authentication is required but missing or invalid).
    /// </summary>
    /// <remarks>
    /// Handlers receive a <see cref="TokenChallengeContext"/> and should return a <see cref="Task"/>.
    /// The default implementation completes immediately.
    /// </remarks>
    public Func<TokenChallengeContext, Task> OnChallenge { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Delegate invoked when a user is signing in with a token.
    /// </summary>
    /// <remarks>
    /// Handlers can inspect or augment the signing-in process via the provided <see cref="TokenSigningInContext"/>.
    /// </remarks>
    public Func<TokenSigningInContext, Task> OnSigningIn { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Delegate invoked when access is forbidden (authorization failed).
    /// </summary>
    /// <remarks>
    /// Handlers receive a <see cref="TokenForbiddenContext"/> and may modify the response or perform logging.
    /// </remarks>
    public Func<TokenForbiddenContext, Task> OnForbidden { get; set; } = context => Task.CompletedTask;

    /// <summary>
    /// Invokes the <see cref="OnChallenge"/> delegate.
    /// </summary>
    /// <param name="context">The challenge context.</param>
    public virtual Task Challenge(TokenChallengeContext context) => OnChallenge(context);

    /// <summary>
    /// Invokes the <see cref="OnSigningIn"/> delegate.
    /// </summary>
    /// <param name="context">The signing-in context.</param>
    public virtual Task SigningIn(TokenSigningInContext context) => OnSigningIn(context);

    /// <summary>
    /// Invokes the <see cref="OnForbidden"/> delegate.
    /// </summary>
    /// <param name="context">The forbidden context.</param>
    public virtual Task Forbidden(TokenForbiddenContext context) => OnForbidden(context);
}