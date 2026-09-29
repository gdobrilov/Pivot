using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Events;
using RubiksCube.Infrastructure.Events;
using RubiksCube.Infrastructure.Persistence;

namespace RubiksCube.Infrastructure;

public static class DependencyInjection
{
    /// <summary>The caller picks the database: a SQLite file for the API, in-memory SQLite for tests.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddDbContext<RubiksDbContext>(configureDatabase);
        services.AddScoped<ICubeSessionRepository, EfCubeSessionRepository>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();

        services.AddSingleton<CubeEventLoggingListener>();
        services.AddSingleton<IDomainEventListener<CubeRotated>>(provider => provider.GetRequiredService<CubeEventLoggingListener>());
        services.AddSingleton<IDomainEventListener<RotationUndone>>(provider => provider.GetRequiredService<CubeEventLoggingListener>());
        services.AddSingleton<IDomainEventListener<CubeReset>>(provider => provider.GetRequiredService<CubeEventLoggingListener>());
        return services;
    }
}
