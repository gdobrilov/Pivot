using RubiksCube.Application.Abstractions;

namespace RubiksCube.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
