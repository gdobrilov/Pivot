using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain.Events;
using RubiksCube.Infrastructure.Events;
using RubiksCube.Infrastructure.Persistence;
using RubiksCube.Infrastructure.Time;

namespace RubiksCube.Infrastructure;

public static class DependencyInjection
{
    /// <summary>The caller picks the database: a SQLite file for the API, in-memory SQLite for tests.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDatabase)
    {
        services.AddDbContext<RubiksDbContext>(configureDatabase);
        services.AddScoped<ICubeSessionRepository, EfCubeSessionRepository>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, InProcessDomainEventDispatcher>();
        services.AddScoped<IDomainEventListener<CubeRotated>, RotationLoggingListener>();
        services.AddScoped<IDomainEventListener<RotationUndone>, RotationLoggingListener>();
        services.AddScoped<IDomainEventListener<CubeReset>, RotationLoggingListener>();
        return services;
    }
}
