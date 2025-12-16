namespace Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;
/// <summary>
/// POCO holding PASETO token options.
/// </summary>
public sealed class PasetoTokenOptions
{
    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public string? PublicKeyBase64 { get; set; }

    public string? SecretKeyBase64 { get; set; }

    public TimeSpan Expiration { get; set; } = TimeSpan.FromDays(1);
}