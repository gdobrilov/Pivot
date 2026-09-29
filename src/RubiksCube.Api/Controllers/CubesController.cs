using Microsoft.AspNetCore.Mvc;
using RubiksCube.Api.Contracts;
using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Rendering;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Api.Controllers;

/// <summary>
/// Thin HTTP layer over the cube use cases. Each action builds one command or query and hands it
/// to its handler; the handler is injected per action so the controller declares only what each
/// route needs.
/// </summary>
[Route("api/cubes")]
[Tags("Cubes")]
public sealed class CubesController : ApiControllerBase
{
    /// <summary>Creates a new cube session in the solved state.</summary>
    [HttpPost]
    [ProducesResponseType<CubeSnapshot>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromServices] ICommandHandler<CreateCubeCommand, CubeSnapshot> handler,
        CancellationToken cancellationToken)
    {
        var snapshot = await handler.HandleAsync(new CreateCubeCommand(), cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = snapshot.Id }, snapshot);
    }

    /// <summary>Returns the current state of a cube session.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CubeSnapshot>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(
        Guid id,
        [FromServices] IQueryHandler<GetCubeQuery, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCubeQuery(id), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Shows what a rotation would do without applying it.</summary>
    [HttpGet("{id:guid}/preview")]
    [ProducesResponseType<RotationPreview>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Preview(
        Guid id,
        [FromQuery] Face face,
        [FromQuery] Rotation rotation,
        [FromServices] IQueryHandler<PreviewRotationQuery, Result<RotationPreview>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new PreviewRotationQuery(id, face, rotation), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Turns a face by 90° clockwise, 90° anti-clockwise or 180°.</summary>
    [HttpPost("{id:guid}/rotations")]
    [ProducesResponseType<CubeSnapshot>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rotate(
        Guid id,
        [FromBody] RotateRequest request,
        [FromServices] ICommandHandler<RotateFaceCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RotateFaceCommand(id, request.Face, request.Rotation), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Returns the session's audit log: every rotation, undo and reset with its time.</summary>
    [HttpGet("{id:guid}/rotations")]
    [ProducesResponseType<IReadOnlyList<RotationLogEntrySnapshot>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLog(
        Guid id,
        [FromServices] IQueryHandler<GetRotationLogQuery, Result<IReadOnlyList<RotationLogEntrySnapshot>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetRotationLogQuery(id), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Reverses the most recent effective rotation.</summary>
    [HttpPost("{id:guid}/rotations/undo")]
    [ProducesResponseType<CubeSnapshot>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Undo(
        Guid id,
        [FromServices] ICommandHandler<UndoRotationCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new UndoRotationCommand(id), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Returns the cube to the solved state; the log is kept.</summary>
    [HttpPost("{id:guid}/reset")]
    [ProducesResponseType<CubeSnapshot>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reset(
        Guid id,
        [FromServices] ICommandHandler<ResetCubeCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ResetCubeCommand(id), cancellationToken);
        return FromResult(result, Ok);
    }

    /// <summary>Renders the cube as a plain-text exploded view, using the same renderer as the console.</summary>
    [HttpGet("{id:guid}/net")]
    [Produces("text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNet(
        Guid id,
        [FromServices] IQueryHandler<GetCubeQuery, Result<CubeSnapshot>> handler,
        [FromServices] ICubeRenderer renderer,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCubeQuery(id), cancellationToken);
        return FromResult(result, snapshot => Content(renderer.Render(ToCube(snapshot.Faces)), "text/plain"));
    }

    private static Cube ToCube(FacesSnapshot faces) =>
        Cube.FromFacelets(string.Concat(
            new[] { faces.Up, faces.Left, faces.Front, faces.Right, faces.Back, faces.Down }
                .SelectMany(face => face)
                .Select(colour => colour.ToSymbol())));
}
