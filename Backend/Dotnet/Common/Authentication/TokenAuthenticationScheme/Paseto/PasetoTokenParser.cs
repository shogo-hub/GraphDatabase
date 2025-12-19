using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;
using Paseto.Protocol;
using System.Security.Claims;

namespace Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;

/// <summary>
/// Parses and validates PASETO tokens according to configured <see cref="PasetoTokenOptions"/>.
/// </summary>
internal sealed class PasetoTokenParser : ITokenParser
{
    private readonly IOptions<PasetoTokenOptions> _options;
    private readonly ILogger<PasetoTokenParser> _logger;
    private readonly PasetoAsymmetricPublicKey? _publicKey;

    /// <summary>
    /// Creates a new <see cref="PasetoTokenParser"/>.
    /// </summary>
    /// <param name="options">PASETO token options (issuer, audience, public key, etc.).</param>
    /// <param name="logger">Logger instance for recording parsing issues.</param>
    public PasetoTokenParser(
        IOptions<PasetoTokenOptions> options,
        ILogger<PasetoTokenParser> logger)
    {
        _options = options;
        _logger = logger;
        _publicKey = options.Value.PublicKeyBase64 == null
            ? null
            : new PasetoAsymmetricPublicKey(Convert.FromBase64String(options.Value.PublicKeyBase64), new Version4());
    }
    
    /// <summary>
    /// Parses a PASETO token string and validates it according to the configured options.
    /// On success returns a <see cref="TokenParseResult"/> with a <see cref="Token"/> containing
    /// claims extracted from the token payload. On failure returns a failed <see cref="TokenParseResult"/>
    /// containing an error message.
    /// </summary>
    /// <param name="token">The compact PASETO token string to parse.</param>
    /// <param name="cancellationToken">Cancellation token (not currently observed by the underlying library).</param>
    /// <returns>A task that completes with the <see cref="TokenParseResult"/> representing parse outcome.</returns>
    public Task<TokenParseResult> ParseAsync(string token, CancellationToken cancellationToken)
    {
        // 1. Read PASETO settings from DI and construct validation parameters used to validate decoded tokens (lifetime, issuer, audience).
        var pasetoValidationParameters = new PasetoTokenValidationParameters
        {
            // Ensure token lifetime is checked.
            ValidateLifetime = true,
            // Only validate audience/issuer when those options are provided.
            ValidateAudience = _options.Value.Audience != null,
            ValidateIssuer = _options.Value.Issuer != null,
            // Pass configured expected audience and issuer for validation.
            ValidAudience = _options.Value.Audience,
            ValidIssuer = _options.Value.Issuer
        };

        // Configure a PasetoBuilder for PASETO v4 public (asymmetric) tokens.
        var builder = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Public);

        // If a public key is configured, attach it so the builder can verify signatures.
        if (_publicKey != null)
        {
            builder = builder.WithKey(_publicKey);
        }

        // Decode and validate the token using the builder and validation parameters.
        var pasetoResult = builder.Decode(token, pasetoValidationParameters);

        TokenParseResult result;

        if (pasetoResult.IsValid)
        {
            // The underlying library occasionally produces a non-fatal Exception
            // even when IsValid is true; log it at information level to aid debugging
            // without treating it as an error.
            if (pasetoResult.Exception != null)
            {
                _logger.LogInformation(pasetoResult.Exception, "Paseto token parsed with non-fatal exception.");
            }

            // On success, map token payload key/value pairs to Claims and return a Token.
            result = TokenParseResult.Success(new Token(
                pasetoResult.Paseto.Payload.Select(x => new Claim(x.Key, x.Value.ToString() ?? string.Empty))));
        }
        else
        {
            // On failure, log warning with the underlying exception and return a failed result
            // containing the exception message or a generic message.
            _logger.LogWarning(pasetoResult.Exception, "Failed to parse paseto token.");
            result = TokenParseResult.Fail(pasetoResult.Exception?.Message ?? "Invalid token");
        }

        // The method is synchronous in practice (library is synchronous), so wrap the
        // result in a completed Task to satisfy the asynchronous interface.
        return Task.FromResult(result);
    }
}