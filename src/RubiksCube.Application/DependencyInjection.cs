using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Rendering;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application;

public static class DependencyInjection
{
    /// <summary>Registers every use case. Each handler is listed explicitly so the set is visible at a glance.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<ICubeRenderer, ExplodedViewRenderer>();

        services.AddScoped<ICommandHandler<CreateCubeCommand, CubeSnapshot>, CreateCubeHandler>();
        services.AddScoped<ICommandHandler<RotateFaceCommand, Result<CubeSnapshot>>, RotateFaceHandler>();
        services.AddScoped<ICommandHandler<UndoRotationCommand, Result<CubeSnapshot>>, UndoRotationHandler>();
        services.AddScoped<ICommandHandler<ResetCubeCommand, Result<CubeSnapshot>>, ResetCubeHandler>();

        services.AddScoped<IQueryHandler<GetCubeQuery, Result<CubeSnapshot>>, GetCubeHandler>();
        services.AddScoped<IQueryHandler<PreviewRotationQuery, Result<RotationPreview>>, PreviewRotationHandler>();
        services.AddScoped<IQueryHandler<GetRotationLogQuery, Result<IReadOnlyList<RotationLogEntrySnapshot>>>, GetRotationLogHandler>();

        return services;
    }
}
