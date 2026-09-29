using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RubiksCube.Api.Contracts;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;

namespace RubiksCube.Api.Tests;

/// <summary>Through HTTP, DI, JSON and the real EF mapping.</summary>
public sealed class CubeEndpointsTests : IClassFixture<ApiFactory>, IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private readonly HttpClient _client;

    public CubeEndpointsTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Create_returns_201_with_location_and_a_solved_cube()
    {
        var response = await _client.PostAsync(new Uri("/api/cubes", UriKind.Relative), content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        Assert.NotNull(cube);
        Assert.True(cube.IsSolved);
        Assert.False(cube.CanUndo);
        Assert.Empty(cube.EffectiveMoves);
        Assert.Equal($"/api/cubes/{cube.Id}", response.Headers.Location?.AbsolutePath);
        Assert.All(cube.Faces.Front, colour => Assert.Equal(Colour.Green, colour));
        Assert.All(cube.Faces.Right, colour => Assert.Equal(Colour.Red, colour));
        Assert.All(cube.Faces.Up, colour => Assert.Equal(Colour.White, colour));
    }

    [Fact]
    public async Task Enums_are_serialised_as_names()
    {
        var response = await _client.PostAsync(new Uri("/api/cubes", UriKind.Relative), content: null);

        var json = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"White\"", json, StringComparison.Ordinal);
        Assert.Contains("\"up\":", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Applying_the_challenge_sequence_returns_the_expected_faces()
    {
        var id = await CreateCubeAsync();
        var sequence = new (Face Face, Rotation Rotation)[]
        {
            (Face.Front, Rotation.Clockwise),
            (Face.Right, Rotation.AntiClockwise),
            (Face.Up, Rotation.Clockwise),
            (Face.Back, Rotation.AntiClockwise),
            (Face.Left, Rotation.Clockwise),
            (Face.Down, Rotation.AntiClockwise),
        };

        CubeSnapshot? cube = null;
        foreach (var (face, rotation) in sequence)
        {
            var response = await _client.PostAsJsonAsync(new Uri($"/api/cubes/{id}/rotations", UriKind.Relative), new RotateRequest(face, rotation), Json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        }

        Assert.NotNull(cube);
        Assert.False(cube.IsSolved);
        Assert.Equal(["F", "R'", "U", "B'", "L", "D'"], cube.EffectiveMoves);
        Assert.Equal(6, cube.Version);
        Assert.Equal(Colours("ROGBWWBBB"), cube.Faces.Up);
        Assert.Equal(Colours("GYYOOGBGO"), cube.Faces.Left);
        Assert.Equal(Colours("ORROGWWWW"), cube.Faces.Front);
        Assert.Equal(Colours("YBORRWOYR"), cube.Faces.Right);
        Assert.Equal(Colours("YBWOBYYYW"), cube.Faces.Back);
        Assert.Equal(Colours("GGBRYRRGG"), cube.Faces.Down);
    }

    [Fact]
    public async Task State_survives_a_round_trip_through_the_database()
    {
        var id = await CreateCubeAsync();
        await RotateAsync(id, Face.Front, Rotation.Half);

        var cube = await _client.GetFromJsonAsync<CubeSnapshot>(new Uri($"/api/cubes/{id}", UriKind.Relative), Json);

        Assert.NotNull(cube);
        Assert.Equal(["F2"], cube.EffectiveMoves);
        Assert.Equal(Colours("WWWWWWYYY"), cube.Faces.Up);
    }

    [Fact]
    public async Task Preview_shows_changes_without_applying_them()
    {
        var id = await CreateCubeAsync();

        var preview = await _client.GetFromJsonAsync<RotationPreview>(new Uri($"/api/cubes/{id}/preview?face=Right&rotation=AntiClockwise", UriKind.Relative), Json);
        var cube = await _client.GetFromJsonAsync<CubeSnapshot>(new Uri($"/api/cubes/{id}", UriKind.Relative), Json);

        Assert.NotNull(preview);
        Assert.Equal("R'", preview.Move);
        Assert.Equal(12, preview.Changes.Count);
        Assert.Equal(Colours("WWBWWBWWB"), preview.After.Up);
        Assert.True(cube!.IsSolved);
    }

    [Fact]
    public async Task Undo_and_reset_are_recorded_in_the_log()
    {
        var id = await CreateCubeAsync();
        await RotateAsync(id, Face.Up, Rotation.Clockwise);
        await RotateAsync(id, Face.Left, Rotation.Clockwise);

        var undone = await _client.PostAsync(new Uri($"/api/cubes/{id}/rotations/undo", UriKind.Relative), content: null);
        var reset = await _client.PostAsync(new Uri($"/api/cubes/{id}/reset", UriKind.Relative), content: null);
        var log = await _client.GetFromJsonAsync<List<RotationLogEntrySnapshot>>(new Uri($"/api/cubes/{id}/rotations", UriKind.Relative), Json);

        Assert.Equal(HttpStatusCode.OK, undone.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        var final = await reset.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        Assert.True(final!.IsSolved);
        Assert.NotNull(log);
        Assert.Equal(["Rotation", "Rotation", "Undo", "Reset"], log.Select(entry => entry.Kind));
        Assert.Equal(["U", "L", "L", null], log.Select(entry => entry.Move));
        Assert.All(log, entry => Assert.Equal(TimeSpan.Zero, entry.OccurredAtUtc.Offset));
    }

    [Fact]
    public async Task Undo_with_nothing_to_undo_returns_400_problem_details()
    {
        var id = await CreateCubeAsync();

        var response = await _client.PostAsync(new Uri($"/api/cubes/{id}/rotations/undo", UriKind.Relative), content: null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("There is nothing to undo.", problem?.Detail);
    }

    [Fact]
    public async Task Unknown_face_name_returns_400()
    {
        var id = await CreateCubeAsync();

        var response = await _client.PostAsJsonAsync(new Uri($"/api/cubes/{id}/rotations", UriKind.Relative), new { face = "Sideways", rotation = "Clockwise" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Unknown_cube_returns_404_problem_details()
    {
        var response = await _client.GetAsync(new Uri($"/api/cubes/{Guid.NewGuid()}", UriKind.Relative));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Net_endpoint_renders_the_exploded_view_as_text()
    {
        var id = await CreateCubeAsync();

        var response = await _client.GetAsync(new Uri($"/api/cubes/{id}/net", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        var text = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("       W W W", text, StringComparison.Ordinal);
        Assert.Contains("O O O  G G G  R R R  B B B", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_endpoint_reports_the_database_as_healthy()
    {
        var response = await _client.GetAsync(new Uri("/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    public void Dispose() => _client.Dispose();

    private async Task<Guid> CreateCubeAsync()
    {
        var response = await _client.PostAsync(new Uri("/api/cubes", UriKind.Relative), content: null);
        var cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        return cube!.Id;
    }

    private async Task RotateAsync(Guid id, Face face, Rotation rotation)
    {
        var response = await _client.PostAsJsonAsync(new Uri($"/api/cubes/{id}/rotations", UriKind.Relative), new RotateRequest(face, rotation), Json);
        response.EnsureSuccessStatusCode();
    }

    private static List<Colour> Colours(string facelets) => facelets.Select(ColourExtensions.FromSymbol).ToList();
}
