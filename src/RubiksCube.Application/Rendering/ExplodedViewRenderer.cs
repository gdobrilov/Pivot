using System.Text;
using RubiksCube.Domain;

namespace RubiksCube.Application.Rendering;

/// <summary>The exploded view: U on top, then L F R B, then D, one letter per sticker.</summary>
public sealed class ExplodedViewRenderer : ICubeRenderer
{
    private const string FaceGap = "  ";
    private static readonly Face[] MiddleRow = [Face.Left, Face.Front, Face.Right, Face.Back];

    public string Render(Cube cube)
    {
        ArgumentNullException.ThrowIfNull(cube);

        var faceWidth = (cube.Size * 2) - 1;
        var indent = new string(' ', faceWidth + FaceGap.Length);
        var builder = new StringBuilder();

        AppendFace(builder, cube, Face.Up, indent);
        for (var row = 0; row < cube.Size; row++)
        {
            builder.AppendJoin(FaceGap, MiddleRow.Select(face => RenderRow(cube, face, row))).AppendLine();
        }

        AppendFace(builder, cube, Face.Down, indent);

        return builder.ToString();
    }

    private static void AppendFace(StringBuilder builder, Cube cube, Face face, string indent)
    {
        for (var row = 0; row < cube.Size; row++)
        {
            builder.Append(indent).Append(RenderRow(cube, face, row)).AppendLine();
        }
    }

    private static string RenderRow(Cube cube, Face face, int row) =>
        string.Join(' ', cube[face].Row(row).Select(ColourExtensions.ToSymbol));
}
