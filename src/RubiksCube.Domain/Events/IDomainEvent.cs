namespace RubiksCube.Domain.Events;

/// <summary>Something that happened to a session. Past tense, never changed.</summary>
public interface IDomainEvent
{
    Guid SessionId { get; }

    DateTimeOffset OccurredAtUtc { get; }
}
