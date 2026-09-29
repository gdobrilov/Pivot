using RubiksCube.Domain;

namespace RubiksCube.Application.Rendering;

public interface ICubeRenderer
{
    string Render(Cube cube);
}
