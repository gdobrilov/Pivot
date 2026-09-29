using RubiksCube.Application.Abstractions;
using RubiksCube.Application.Messaging;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Cubes;

public sealed class CreateCubeHandler(ICubeSessionRepository repository, TimeProvider timeProvider)
    : ICommandHandler<CreateCubeCommand, CubeSnapshot>
{
    public async Task<CubeSnapshot> HandleAsync(CreateCubeCommand command, CancellationToken cancellationToken = default)
    {
        var session = CubeSession.Create(timeProvider.GetUtcNow());
        repository.Add(session);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return session.ToSnapshot();
    }
}
