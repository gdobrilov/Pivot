using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Common;
using RubiksCube.Application.Cubes;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Rendering;
using RubiksCube.Application.Snapshots;

namespace RubiksCube.Application;

public static class DependencyInjection
{
    /// <summary>Handlers are listed one by one so the full set of use cases is visible here.</summary>
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
        services.AddScoped<IQueryHandler<RenderCubeQuery, Result<string>>, RenderCubeHandler>();

        return services;
    }
}
