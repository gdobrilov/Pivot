namespace RubiksCube.Application.Common;

/// <summary>An expected failure, returned rather than thrown.</summary>
public sealed record ResultError(ErrorType Type, string Message)
{
    public static ResultError Validation(string message) => new(ErrorType.Validation, message);

    public static ResultError NotFound(string message) => new(ErrorType.NotFound, message);

    public static ResultError Conflict(string message) => new(ErrorType.Conflict, message);

    public static ResultError SessionNotFound(Guid id) => NotFound($"Cube session '{id}' was not found.");
}
