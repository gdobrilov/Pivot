using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

namespace RubiksCube.Application.Snapshots;

public static class SnapshotMapper
{
    public static CubeSnapshot ToSnapshot(this CubeSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new CubeSnapshot(
            session.Id,
            session.Cube.IsSolved,
            session.Version,
            session.CanUndo,
            session.EffectiveMoves.Select(MoveNotation.Format).ToList(),
            session.Cube.ToFacesSnapshot());
    }

    public static FacesSnapshot ToFacesSnapshot(this Cube cube)
    {
        ArgumentNullException.ThrowIfNull(cube);
        return new FacesSnapshot(
            cube[Face.Up].Cells,
            cube[Face.Left].Cells,
            cube[Face.Front].Cells,
            cube[Face.Right].Cells,
            cube[Face.Back].Cells,
            cube[Face.Down].Cells);
    }

    public static RotationPreview ToPreview(this Cube before, Move move)
    {
        ArgumentNullException.ThrowIfNull(before);
        var after = before.Turn(move);
        var changes = new List<StickerChange>();
        foreach (var face in Faces.All)
        {
            for (var row = 0; row < before.Size; row++)
            {
                for (var column = 0; column < before.Size; column++)
                {
                    var from = before[face, row, column];
                    var to = after[face, row, column];
                    if (from != to)
                    {
                        changes.Add(new StickerChange(face, row, column, from, to));
                    }
                }
            }
        }

        return new RotationPreview(MoveNotation.Format(move), after.ToFacesSnapshot(), changes);
    }

    public static RotationLogEntrySnapshot ToSnapshot(this RotationLogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return new RotationLogEntrySnapshot(
            entry.Sequence,
            entry.Kind,
            entry.Face,
            entry.Rotation,
            entry.Move is { } move ? MoveNotation.Format(move) : null,
            entry.OccurredAtUtc);
    }
}
