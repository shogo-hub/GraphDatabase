namespace Backend.Common.Authentication.TokenAuthenticationScheme.Paseto;

public sealed class PasetoTokenOptions
{
    public string? Issuer { get; set; }

    public string? Audience { get; set; }

    public string? PublicKeyBase64 { get; set; }

    public string? SecretKeyBase64 { get; set; }

    public TimeSpan Exipration { get; set; } = TimeSpan.FromDays(1);
}