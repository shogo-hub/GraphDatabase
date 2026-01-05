namespace Backend.Dotnet.Common.Errors.Types;

public sealed class AuthenticationFailedError : Error
{
    public AuthenticationFailedError() : base(
        "common.authentication_failed",
        "User credentials are missing or invalid.")
    {
    }

    public override string Title => "Authentication failed";
}