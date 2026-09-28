using RubiksCube.Domain;

namespace RubiksCube.Application.Rendering;

/// <summary>Turns a cube into a textual representation.</summary>
public interface ICubeRenderer
{
    string Render(Cube cube);
}
