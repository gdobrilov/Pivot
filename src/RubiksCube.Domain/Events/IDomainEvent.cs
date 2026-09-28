namespace RubiksCube.Domain.Events;

/// <summary>Something that happened in the domain, stated in the past tense and never changed.</summary>
public interface IDomainEvent
{
    Guid SessionId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}

/// <summary>A face was turned.</summary>
public sealed record CubeRotated(Guid SessionId, int Sequence, Move Move, DateTimeOffset OccurredAtUtc) : IDomainEvent;

/// <summary>The most recent effective move was undone by applying its inverse.</summary>
public sealed record RotationUndone(Guid SessionId, int Sequence, Move UndoneMove, DateTimeOffset OccurredAtUtc) : IDomainEvent;

/// <summary>The cube was returned to the solved state.</summary>
public sealed record CubeReset(Guid SessionId, int Sequence, DateTimeOffset OccurredAtUtc) : IDomainEvent;
