using Microsoft.AspNetCore.Mvc;
using RubiksCube.Application.Common;

namespace RubiksCube.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Value on success, problem details with the matching status on failure.</summary>
    protected IActionResult FromResult<T>(Result<T> result, Func<T, IActionResult> onSuccess)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.Match(onSuccess, ToProblem);
    }

    private IActionResult ToProblem(ResultError error) => error.Type switch
    {
        ErrorType.NotFound => Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: error.Message),
        ErrorType.Validation => Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid request", detail: error.Message),
        ErrorType.Conflict => Problem(statusCode: StatusCodes.Status409Conflict, title: "Conflict", detail: error.Message),
        _ => Problem(statusCode: StatusCodes.Status500InternalServerError, detail: error.Message),
    };
}
