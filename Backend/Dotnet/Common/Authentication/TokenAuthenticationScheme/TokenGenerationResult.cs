namespace Backend.Common.Authentication.TokenAuthenticationScheme;

public sealed class TokenGenerationResult(string accessToken)
{
    public string AccessToken { get; } = accessToken;
}