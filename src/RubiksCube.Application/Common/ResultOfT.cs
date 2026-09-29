namespace RubiksCube.Application.Common;

/// <summary>Either a value or a <see cref="ResultError"/>.</summary>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly ResultError? _error;

    internal Result(T value)
    {
        _value = value;
        _error = null;
    }

    internal Result(ResultError error)
    {
        _value = default;
        _error = error;
    }

    public bool IsSuccess => _error is null;

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public ResultError Error => _error ?? throw new InvalidOperationException("A successful result has no error.");

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ResultError, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess ? onSuccess(_value!) : onFailure(_error!);
    }

    public static implicit operator Result<T>(T value) => Result.Success(value);

    public static implicit operator Result<T>(ResultError error) => Result.Failure<T>(error);
}
