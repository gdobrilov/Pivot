namespace RubiksCube.Domain.Events;

/// <summary>Something that happened to a session. Past tense, never changed.</summary>
public interface IDomainEvent
{
    Guid SessionId { get; }

    /// <summary>The log entry this event belongs to.</summary>
    int Sequence { get; }

    DateTimeOffset OccurredAtUtc { get; }
}
