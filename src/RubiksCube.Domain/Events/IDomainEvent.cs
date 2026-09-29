namespace RubiksCube.Domain.Events;

/// <summary>Something that happened to a session. Past tense, never changed.</summary>
public interface IDomainEvent
{
    Guid SessionId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}

public sealed record CubeRotated(Guid SessionId, int Sequence, Move Move, DateTimeOffset OccurredAtUtc) : IDomainEvent;

public sealed record RotationUndone(Guid SessionId, int Sequence, Move UndoneMove, DateTimeOffset OccurredAtUtc) : IDomainEvent;

public sealed record CubeReset(Guid SessionId, int Sequence, DateTimeOffset OccurredAtUtc) : IDomainEvent;
