namespace RubiksCube.Domain.Events;

public sealed record CubeReset(Guid SessionId, int Sequence, DateTimeOffset OccurredAtUtc) : IDomainEvent;
