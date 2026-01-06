namespace Backend.Dotnet.Common.Miscellaneous;

/// <summary>
/// Represents a failed operation result that carries an error value.
/// </summary>
/// <typeparam name="TError">The type of the error value.</typeparam>
public readonly struct FailedResult<TError>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FailedResult{TError}"/> struct
    /// with the specified error value.
    /// </summary>
    /// <param name="error">The error value associated with the failed result.</param>
    public FailedResult(TError error)
    {
        Error = error;
    }

    /// <summary>
    /// Gets the error associated with the failed result.
    /// </summary>
    public TError Error { get; }
}