using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;
using Paseto.Protocol;
using System.Security.Claims;

namespace Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;

internal sealed class PasetoTokenParser : ITokenParser
{
    private readonly IOptions<PasetoTokenOptions> _options;
    private readonly ILogger<PasetoTokenParser> _logger;
    private readonly PasetoAsymmetricPublicKey? _publicKey;

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

    public Task<TokenParseResult> ParseAsync(string token, CancellationToken cancellationToken)
    {
        var pasetoValidationParameters = new PasetoTokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateAudience = _options.Value.Audience != null,
            ValidateIssuer = _options.Value.Issuer != null,
            ValidAudience = _options.Value.Audience,
            ValidIssuer = _options.Value.Issuer
        };

        var builder = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Public);

        if (_publicKey != null)
        {
            builder = builder.WithKey(_publicKey);
        }

        var pasetoResult = builder.Decode(token, pasetoValidationParameters);

        TokenParseResult result;

        if (pasetoResult.IsValid)
        {
            _logger.LogWarning(pasetoResult.Exception, "Failed to parse paseto token.");
            result = TokenParseResult.Success(new Token(
                pasetoResult.Paseto.Payload.Select(x => new Claim(x.Key, x.Value.ToString() ?? ""))));
        }
        else
        {
            result = TokenParseResult.Fail(pasetoResult.Exception?.Message ?? "Invalid token");
        }

        return Task.FromResult(result);
    }
}