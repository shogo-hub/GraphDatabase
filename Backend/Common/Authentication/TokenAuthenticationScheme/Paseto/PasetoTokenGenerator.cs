using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Paseto;
using Paseto.Builder;
using Paseto.Cryptography.Key;
using Paseto.Protocol;
using System.Security.Claims;

namespace Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;

internal sealed class PasetoTokenGenerator : ITokenGenerator
{
    private readonly IOptions<PasetoTokenOptions> _options;
    private readonly TimeProvider? _timeProvider;
    private readonly PasetoAsymmetricSecretKey? _secretKey;

    public PasetoTokenGenerator(
        IOptions<PasetoTokenOptions> options,
        TimeProvider? timeProvider = null)
    {
        _options = options;
        _timeProvider = timeProvider;
        _secretKey = options.Value.SecretKeyBase64 == null
            ? null
            : new PasetoAsymmetricSecretKey(Convert.FromBase64String(options.Value.SecretKeyBase64), new Version4());
    }

    public Task<TokenGenerationResult> GenerateTokensAsync(
        ClaimsPrincipal user,
        AuthenticationProperties? properties,
        CancellationToken cancellationToken)
    {
        var builder = new PasetoBuilder().Use(ProtocolVersion.V4, Purpose.Public);

        if (_secretKey != null)
        {
            builder = builder.WithKey(_secretKey);
        }

        foreach (var claim in user.Claims)
        {
            builder.AddClaim(claim.Type, claim.Value);
        }

        var issuedAt = _timeProvider?.GetUtcNow() ?? DateTimeOffset.UtcNow;

        var accessToken = builder
            .IssuedAt(issuedAt)
            .NotBefore(issuedAt.AddMinutes(-5))
            .Expiration(issuedAt.Add(_options.Value.Exipration))
            .AddFooter(Guid.NewGuid().ToString())
            .Encode();

        var result = new TokenGenerationResult(accessToken);
        return Task.FromResult(result);
    }
}