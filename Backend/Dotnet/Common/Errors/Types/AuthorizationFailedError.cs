namespace Backend.Dotnet.Common.Errors.Types;

public sealed class AuthorizationFailedError : Error
{
    public AuthorizationFailedError(string detail) : base("common.authorization_failed", detail)
    {
    }

    public override string Title => "Authorization failed";
}