namespace Backend.Dotnet.Common.Miscellaneous;

/// <summary>
/// Represents a successful operation result that carries a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public readonly struct SucceededResult<TValue>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SucceededResult{TValue}"/> struct
    /// with the specified value.
    /// </summary>
    /// <param name="value">The value associated with the successful result.</param>
    public SucceededResult(TValue value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the value associated with the successful result.
    /// </summary>
    public TValue Value { get; }
}