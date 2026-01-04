namespace Backend.Dotnet.Common.Errors.Types;

/// <summary>
/// Abstract class for Error Model classes
/// </summary>
public abstract class Error
{
    protected Error(string code, string detail, object? parameters = null)
    {
        Code = code;
        Detail = detail;
        Parameters = parameters;
    }

    public abstract string Title { get; }

    public string Code { get; }

    public string Detail { get; }

    public virtual object? Parameters { get; }

    public override string ToString()
    {
        return $"{Code}: {Detail}";
    }
}