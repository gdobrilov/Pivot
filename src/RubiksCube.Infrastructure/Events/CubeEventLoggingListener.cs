using Microsoft.Extensions.Logging;
using RubiksCube.Application.Abstractions;
using RubiksCube.Domain;
using RubiksCube.Domain.Events;

namespace RubiksCube.Infrastructure.Events;

/// <summary>Logs each event. A bus publisher or metrics would hang off the same events.</summary>
public sealed partial class CubeEventLoggingListener(ILogger<CubeEventLoggingListener> logger)
    : IDomainEventListener<CubeRotated>, IDomainEventListener<RotationUndone>, IDomainEventListener<CubeReset>
{
    public Task HandleAsync(CubeRotated domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        LogRotated(logger, domainEvent.SessionId, domainEvent.Sequence, domainEvent.Move);
        return Task.CompletedTask;
    }

    public Task HandleAsync(RotationUndone domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        LogUndone(logger, domainEvent.SessionId, domainEvent.Sequence, domainEvent.UndoneMove);
        return Task.CompletedTask;
    }

    public Task HandleAsync(CubeReset domainEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        LogReset(logger, domainEvent.SessionId, domainEvent.Sequence);
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Cube {SessionId} rotated {Move} (#{Sequence})")]
    private static partial void LogRotated(ILogger logger, Guid sessionId, int sequence, Move move);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cube {SessionId} undid {Move} (#{Sequence})")]
    private static partial void LogUndone(ILogger logger, Guid sessionId, int sequence, Move move);

    [LoggerMessage(Level = LogLevel.Information, Message = "Cube {SessionId} reset (#{Sequence})")]
    private static partial void LogReset(ILogger logger, Guid sessionId, int sequence);
}
