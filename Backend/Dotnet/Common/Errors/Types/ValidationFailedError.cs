namespace Backend.Dotnet.Common.Errors.Types;

public class ValidationFailedError : Error
{
    public ValidationFailedError(string detail) : this("common.validation_failed", detail)
    {
    }

    protected ValidationFailedError(string code, string detail) : base(code, detail)
    {
    }

    public override string Title => "Validation failed";
}