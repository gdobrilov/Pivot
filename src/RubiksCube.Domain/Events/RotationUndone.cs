namespace RubiksCube.Domain.Events;

public sealed record RotationUndone(Guid SessionId, int Sequence, Move UndoneMove, DateTimeOffset OccurredAtUtc) : IDomainEvent;
