namespace Backend.Common.Authentication.TokenAuthenticationScheme;

public interface ITokenParser
{
    Task<TokenParseResult> ParseAsync(string token, CancellationToken cancellationToken);
}