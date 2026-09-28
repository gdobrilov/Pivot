using Microsoft.AspNetCore.Http.HttpResults;
using RubiksCube.Api.Contracts;
using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Rendering;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Api.Endpoints;

/// <summary>Thin HTTP layer: each endpoint builds a command or query and hands it to its handler.</summary>
internal static class CubeEndpoints
{
    public static IEndpointRouteBuilder MapCubeEndpoints(this IEndpointRouteBuilder app)
    {
        var cubes = app.MapGroup("/api/cubes").WithTags("Cubes");

        cubes.MapPost("/", CreateAsync)
            .WithName("CreateCube")
            .WithSummary("Creates a new cube session in the solved state.");

        cubes.MapGet("/{id:guid}", GetAsync)
            .WithName("GetCube")
            .WithSummary("Returns the current state of a cube session.");

        cubes.MapGet("/{id:guid}/preview", PreviewAsync)
            .WithName("PreviewRotation")
            .WithSummary("Shows what a rotation would do without applying it.");

        cubes.MapPost("/{id:guid}/rotations", RotateAsync)
            .WithName("RotateFace")
            .WithSummary("Turns a face by 90° clockwise, 90° anti-clockwise or 180°.");

        cubes.MapGet("/{id:guid}/rotations", GetLogAsync)
            .WithName("GetRotationLog")
            .WithSummary("Returns the session's audit log: every rotation, undo and reset with its time.");

        cubes.MapPost("/{id:guid}/rotations/undo", UndoAsync)
            .WithName("UndoRotation")
            .WithSummary("Reverses the most recent effective rotation.");

        cubes.MapPost("/{id:guid}/reset", ResetAsync)
            .WithName("ResetCube")
            .WithSummary("Returns the cube to the solved state; the log is kept.");

        cubes.MapGet("/{id:guid}/net", GetNetAsync)
            .WithName("GetCubeNet")
            .WithSummary("Renders the cube as a plain-text exploded view.");

        return app;
    }

    private static async Task<CreatedAtRoute<CubeSnapshot>> CreateAsync(
        ICommandHandler<CreateCubeCommand, CubeSnapshot> handler,
        CancellationToken cancellationToken)
    {
        var snapshot = await handler.HandleAsync(new CreateCubeCommand(), cancellationToken);
        return TypedResults.CreatedAtRoute(snapshot, "GetCube", new { id = snapshot.Id });
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        IQueryHandler<GetCubeQuery, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCubeQuery(id), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> PreviewAsync(
        Guid id,
        Face face,
        Rotation rotation,
        IQueryHandler<PreviewRotationQuery, Result<RotationPreview>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new PreviewRotationQuery(id, face, rotation), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> RotateAsync(
        Guid id,
        RotateRequest request,
        ICommandHandler<RotateFaceCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RotateFaceCommand(id, request.Face, request.Rotation), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> GetLogAsync(
        Guid id,
        IQueryHandler<GetRotationLogQuery, Result<IReadOnlyList<RotationLogEntrySnapshot>>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetRotationLogQuery(id), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> UndoAsync(
        Guid id,
        ICommandHandler<UndoRotationCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new UndoRotationCommand(id), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> ResetAsync(
        Guid id,
        ICommandHandler<ResetCubeCommand, Result<CubeSnapshot>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new ResetCubeCommand(id), cancellationToken);
        return result.ToHttpResult(TypedResults.Ok);
    }

    private static async Task<IResult> GetNetAsync(
        Guid id,
        IQueryHandler<GetCubeQuery, Result<CubeSnapshot>> handler,
        ICubeRenderer renderer,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetCubeQuery(id), cancellationToken);
        return result.ToHttpResult(snapshot => TypedResults.Text(renderer.Render(snapshot.Faces.ToCube())));
    }

    private static Cube ToCube(this FacesSnapshot faces) =>
        Cube.FromFacelets(string.Concat(
            new[] { faces.Up, faces.Left, faces.Front, faces.Right, faces.Back, faces.Down }
                .SelectMany(face => face)
                .Select(colour => colour.ToSymbol())));
}
