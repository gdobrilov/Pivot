namespace RubiksCube.Application.Common;

public static class Result
{
    public static Result<T> Success<T>(T value) => new(value);

    public static Result<T> Failure<T>(ResultError error) => new(error);
}
