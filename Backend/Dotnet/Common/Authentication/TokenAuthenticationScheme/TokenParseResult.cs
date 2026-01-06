using System.Diagnostics.CodeAnalysis;

namespace Backend.Dotnet.Common.Authentication.TokenAuthenticationScheme;

public readonly struct TokenParseResult
{
    private TokenParseResult(Token? value, string? error)
    {
        Value = value;
        Error = error;
    }

    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSucceeded => Value != null;

    public Token? Value { get; }

    public string? Error { get; }

    public static TokenParseResult Success(Token value)
    {
        return new(value, null);
    }

    public static TokenParseResult Fail(string error)
    {
        return new(null, error);
    }
}