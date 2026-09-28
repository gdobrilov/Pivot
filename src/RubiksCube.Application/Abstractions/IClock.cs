namespace RubiksCube.Application.Abstractions;

/// <summary>Source of the current time, so that timestamps in the log are testable and always UTC.</summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
