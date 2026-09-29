namespace RubiksCube.Domain.Events;

public sealed record CubeRotated(Guid SessionId, int Sequence, Move Move, DateTimeOffset OccurredAtUtc) : IDomainEvent;
