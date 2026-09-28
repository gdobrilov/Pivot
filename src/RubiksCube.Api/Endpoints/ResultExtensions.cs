using RubiksCube.Application.Common;

namespace RubiksCube.Api.Endpoints;

internal static class ResultExtensions
{
    /// <summary>Maps a use-case result to HTTP: the value on success, RFC 9457 problem details on failure.</summary>
    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.Match(onSuccess, ToProblem);

    private static IResult ToProblem(ResultError error) => error.Type switch
    {
        ErrorType.NotFound => Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: error.Message),
        ErrorType.Validation => Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: error.Message),
        ErrorType.Conflict => Results.Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: error.Message),
        _ => Results.Problem(statusCode: StatusCodes.Status500InternalServerError, detail: error.Message),
    };
}
