namespace RubiksCube.Application.Abstractions;

/// <summary>Injected so tests can fix the time and the log is always UTC.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
