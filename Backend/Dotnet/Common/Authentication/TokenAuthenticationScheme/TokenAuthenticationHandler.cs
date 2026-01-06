using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

/// <summary>
/// Authentication handler that validates and issues token-based authentication.
/// </summary>
/// <param name="options">Options monitor for <see cref="TokenAuthenticationOptions"/>.</param>
/// <param name="logger">Logger factory for handler diagnostics.</param>
/// <param name="encoder">URL encoder used by the base handler.</param>
/// <param name="tokenAccessor">Implementation responsible for reading/writing tokens (cookies, headers, etc.).</param>
/// <param name="tokenParser">Parser that validates and parses incoming access tokens.</param>
/// <param name="tokenGenerator">Generator that creates tokens when signing in users.</param>
public sealed class TokenAuthenticationHandler(
    IOptionsMonitor<TokenAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    ITokenAccessor tokenAccessor,
    ITokenParser tokenParser,
    ITokenGenerator tokenGenerator) :
    SignInAuthenticationHandler<TokenAuthenticationOptions>(options, logger, encoder)
{
    /// <summary>
    /// Strongly-typed events for this authentication scheme.
    /// </summary>
    private new TokenAuthenticationEvents Events
    {
        get => (TokenAuthenticationEvents)base.Events!;
        set => base.Events = value;
    }

    /// <summary>
    /// Attempts to authenticate the current request using an access token obtained
    /// from the configured <see cref="ITokenAccessor"/> and parsed by <see cref="ITokenParser"/>.
    /// </summary>
    /// <returns>An <see cref="AuthenticateResult"/> describing success or failure.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var accessToken = tokenAccessor.GetAccessTokenOrDefault(Request);

        if (accessToken == null)
        {
            return AuthenticateResult.Fail("No access token provided.");
        }

        var tokenParseResult = await tokenParser.ParseAsync(accessToken, Context.RequestAborted);

        if (!tokenParseResult.IsSucceeded)
        {
            return AuthenticateResult.Fail(tokenParseResult.Error);
        }

        var principal = new ClaimsPrincipal(new[]
        {
            new ClaimsIdentity(tokenParseResult.Value.Value.Claims, Scheme.Name)
        });
        return AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name));
    }

    /// <summary>
    /// Handles signing in a user by generating tokens and storing the access token
    /// using the configured <see cref="ITokenAccessor"/>.
    /// </summary>
    /// <param name="user">The principal that is being signed in.</param>
    /// <param name="properties">Optional authentication properties.</param>
    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var signingInContext = new TokenSigningInContext(Context, Scheme, Options, properties, user);
        await Events.SigningIn(signingInContext);

        if (signingInContext.Handled)
        {
            return;
        }

        var tokens = await tokenGenerator.GenerateTokensAsync(
            signingInContext.User,
            properties,
            Context.RequestAborted);
        tokenAccessor.SetAccessToken(Response, tokens.AccessToken);
    }

    /// <summary>
    /// Handles sign-out by removing any stored tokens via <see cref="ITokenAccessor"/>.
    /// </summary>
    /// <param name="properties">Optional authentication properties.</param>
    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        tokenAccessor.DeleteTokens(Response);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles an authentication challenge (when an unauthenticated request requires credentials).
    /// </summary>
    /// <param name="properties">The <see cref="AuthenticationProperties"/> for the challenge.</param>
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var challengeContext = new TokenChallengeContext(Context, Scheme, Options, properties);
        await Events.Challenge(challengeContext);

        if (challengeContext.Handled)
        {
            return;
        }

        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
    }

    /// <summary>
    /// Handles a forbidden response (when authentication succeeded but authorization failed).
    /// </summary>
    /// <param name="properties">The <see cref="AuthenticationProperties"/> associated with the request.</param>
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var forbiddenContext = new TokenForbiddenContext(Context, Scheme, Options, properties);
        await Events.Forbidden(forbiddenContext);

        if (forbiddenContext.Handled)
        {
            return;
        }

        await base.HandleForbiddenAsync(properties);
    }
}