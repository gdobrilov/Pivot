using RubiksCube.Domain;

namespace RubiksCube.Application.Snapshots;

public sealed record StickerChange(Face Face, int Row, int Column, Colour From, Colour To);
