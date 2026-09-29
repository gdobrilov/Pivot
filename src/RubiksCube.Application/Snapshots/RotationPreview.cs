namespace RubiksCube.Application.Snapshots;

/// <summary>What a move would do, without doing it.</summary>
public sealed record RotationPreview(string Move, FacesSnapshot After, IReadOnlyList<StickerChange> Changes);
