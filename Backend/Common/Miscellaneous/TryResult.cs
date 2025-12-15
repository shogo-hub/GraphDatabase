using System.Diagnostics.CodeAnalysis;

namespace Backend.Common.Miscellaneous;

/// <summary>
/// Represents the result of an operation that may either succeed with a value
/// of type <typeparamref name="TValue"/> or fail with an error of type
/// <typeparamref name="TError"/>.
/// </summary>
/// <typeparam name="TValue">The type of the value when the operation succeeds.</typeparam>
/// <typeparam name="TError">The type of the error when the operation fails.</typeparam>
public sealed class TryResult<TValue, TError>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TryResult{TValue,TError}"/> class.
    /// </summary>
    /// <param name="value">The success value, or <c>default</c> when failed.</param>
    /// <param name="error">The error value, or <c>default</c> when succeeded.</param>
    /// <param name="isSucceeded">True when the result is successful; otherwise false.</param>
    private TryResult(TValue? value, TError? error, bool isSucceeded)
    {
        Value = value;
        Error = error;
        IsSucceeded = isSucceeded;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// When <c>true</c>, <see cref="Value"/> is not null; when <c>false</c>, <see cref="Error"/> is not null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSucceeded { get; }

    /// <summary>
    /// Gets the success value when <see cref="IsSucceeded"/> is <c>true</c>; otherwise <c>null</c>.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Gets the error value when <see cref="IsSucceeded"/> is <c>false</c>; otherwise <c>null</c>.
    /// </summary>
    public TError? Error { get; }

    /// <summary>
    /// Returns the success value or throws an <see cref="InvalidOperationException"/>
    /// when the result represents a failure.
    /// </summary>
    /// <returns>The success value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public TValue GetValueOrThrow()
    {
        return IsSucceeded ? Value : throw new InvalidOperationException(Error?.ToString());
    }

    /// <summary>
    /// Returns the error value or throws an <see cref="InvalidOperationException"/>
    /// when the result represents a success.
    /// </summary>
    /// <returns>The error value.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the result is a success.</exception>
    public TError GetErrorOrThrow()
    {
        return IsSucceeded ? throw new InvalidOperationException("The result is successful.") : Error;
    }

    /// <summary>
    /// Creates a successful <see cref="TryResult{TValue,TError}"/> containing the specified <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The success value.</param>
    /// <returns>A successful <see cref="TryResult{TValue,TError}"/>.</returns>
    public static TryResult<TValue, TError> Succeed(TValue value)
    {
        return new TryResult<TValue, TError>(value, default, true);
    }

    /// <summary>
    /// Creates a failed <see cref="TryResult{TValue,TError}"/> containing the specified <paramref name="error"/>.
    /// </summary>
    /// <param name="error">The error value.</param>
    /// <returns>A failed <see cref="TryResult{TValue,TError}"/>.</returns>
    public static TryResult<TValue, TError> Fail(TError error)
    {
        return new TryResult<TValue, TError>(default, error, false);
    }

    /// <summary>
    /// Implicit conversion from a <see cref="SucceededResult{TValue}"/> to a
    /// successful <see cref="TryResult{TValue,TError}"/>.
    /// </summary>
    /// <param name="value">The succeeded wrapper.</param>
    public static implicit operator TryResult<TValue, TError>(SucceededResult<TValue> value)
    {
        return Succeed(value.Value);
    }

    /// <summary>
    /// Implicit conversion from a <see cref="FailedResult{TError}"/> to a
    /// failed <see cref="TryResult{TValue,TError}"/>.
    /// </summary>
    /// <param name="value">The failed wrapper.</param>
    public static implicit operator TryResult<TValue, TError>(FailedResult<TError> value)
    {
        return Fail(value.Error);
    }
}

/// <summary>
/// Factory helpers for creating <see cref="SucceededResult{TValue}"/> and
/// <see cref="FailedResult{TError}"/> instances.
/// </summary>
public static class TryResult
{
    /// <summary>
    /// Creates a <see cref="SucceededResult{TValue}"/> wrapping the provided <paramref name="value"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type.</typeparam>
    /// <param name="value">The success value.</param>
    /// <returns>A <see cref="SucceededResult{TValue}"/>.</returns>
    public static SucceededResult<TValue> Succeed<TValue>(TValue value)
    {
        return new SucceededResult<TValue>(value);
    }

    /// <summary>
    /// Creates a <see cref="FailedResult{TError}"/> wrapping the provided <paramref name="error"/>.
    /// </summary>
    /// <typeparam name="TError">The error type.</typeparam>
    /// <param name="error">The error value.</param>
    /// <returns>A <see cref="FailedResult{TError}"/>.</returns>
    public static FailedResult<TError> Fail<TError>(TError error)
    {
        return new FailedResult<TError>(error);
    }
}