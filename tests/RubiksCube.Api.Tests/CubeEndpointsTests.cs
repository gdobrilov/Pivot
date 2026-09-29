using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RubiksCube.Api.Contracts;
using RubiksCube.Application.Snapshots;
using RubiksCube.Domain;
using RubiksCube.Domain.Sessions;

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
        var response = await _client.PostAsync(Url("/api/cubes"), content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        Assert.NotNull(cube);
        Assert.True(cube.IsSolved);
        Assert.False(cube.CanUndo);
        Assert.Equal($"/api/cubes/{cube.Id}", response.Headers.Location?.AbsolutePath);
        Assert.All(cube.Faces.Front, colour => Assert.Equal(Colour.Green, colour));
    }

    [Fact]
    public async Task Enums_are_serialised_as_names()
    {
        var response = await _client.PostAsync(Url("/api/cubes"), content: null);

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
            var response = await _client.PostAsJsonAsync(Url($"/api/cubes/{id}/rotations"), new RotateRequest(face, rotation), Json);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        }

        Assert.NotNull(cube);
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

        var cube = await _client.GetFromJsonAsync<CubeSnapshot>(Url($"/api/cubes/{id}"), Json);

        Assert.Equal(["F2"], cube!.EffectiveMoves);
        Assert.Equal(Colours("WWWWWWYYY"), cube.Faces.Up);
    }

    [Fact]
    public async Task Preview_shows_changes_without_applying_them()
    {
        var id = await CreateCubeAsync();

        var preview = await _client.GetFromJsonAsync<RotationPreview>(Url($"/api/cubes/{id}/preview?face=Right&rotation=AntiClockwise"), Json);
        var cube = await _client.GetFromJsonAsync<CubeSnapshot>(Url($"/api/cubes/{id}"), Json);

        Assert.Equal("R'", preview!.Move);
        Assert.Equal(12, preview.Changes.Count);
        Assert.True(cube!.IsSolved);
    }

    [Fact]
    public async Task Undo_and_reset_are_recorded_in_the_log()
    {
        var id = await CreateCubeAsync();
        await RotateAsync(id, Face.Up, Rotation.Clockwise);
        await RotateAsync(id, Face.Left, Rotation.Clockwise);

        var undone = await _client.PostAsync(Url($"/api/cubes/{id}/undo"), content: null);
        var reset = await _client.PostAsync(Url($"/api/cubes/{id}/reset"), content: null);
        var log = await _client.GetFromJsonAsync<List<RotationLogEntrySnapshot>>(Url($"/api/cubes/{id}/rotations"), Json);

        Assert.Equal(HttpStatusCode.OK, undone.StatusCode);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.NotNull(log);
        Assert.Equal([LogEntryKind.Rotation, LogEntryKind.Rotation, LogEntryKind.Undo, LogEntryKind.Reset], log.Select(entry => entry.Kind));
        Assert.Equal(["U", "L", "L", null], log.Select(entry => entry.Move));
        Assert.All(log, entry => Assert.Equal(TimeSpan.Zero, entry.OccurredAtUtc.Offset));
    }

    [Fact]
    public async Task Undo_with_nothing_to_undo_returns_409()
    {
        var id = await CreateCubeAsync();

        var response = await _client.PostAsync(Url($"/api/cubes/{id}/undo"), content: null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("There is nothing to undo.", problem?.Detail);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"face\":\"Front\"}")]
    [InlineData("{\"face\":\"Sideways\",\"rotation\":\"Clockwise\"}")]
    [InlineData("{\"face\":1,\"rotation\":\"Clockwise\"}")]
    public async Task A_missing_or_invalid_face_or_rotation_returns_400(string body)
    {
        var id = await CreateCubeAsync();

        var response = await _client.PostAsync(Url($"/api/cubes/{id}/rotations"), new StringContent(body, System.Text.Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var cube = await _client.GetFromJsonAsync<CubeSnapshot>(Url($"/api/cubes/{id}"), Json);
        Assert.True(cube!.IsSolved);
    }

    [Theory]
    [InlineData("")]
    [InlineData("?face=Front")]
    [InlineData("?rotation=Half")]
    public async Task Preview_without_both_parameters_returns_400(string query)
    {
        var id = await CreateCubeAsync();

        var response = await _client.GetAsync(Url($"/api/cubes/{id}/preview{query}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("POST", "/undo")]
    [InlineData("POST", "/reset")]
    [InlineData("GET", "/rotations")]
    [InlineData("GET", "/net")]
    public async Task Unknown_cube_returns_404_problem_details(string method, string path)
    {
        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), Url($"/api/cubes/{Guid.NewGuid()}{path}")));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Net_endpoint_renders_the_exploded_view_as_text()
    {
        var id = await CreateCubeAsync();

        var response = await _client.GetAsync(Url($"/api/cubes/{id}/net"));

        Assert.Equal("text/plain", response.Content.Headers.ContentType?.MediaType);
        var text = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("       W W W\n", text, StringComparison.Ordinal);
        Assert.Contains("O O O  G G G  R R R  B B B\n", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Health_endpoint_reports_the_database_as_healthy()
    {
        var response = await _client.GetAsync(Url("/health"));

        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    public void Dispose() => _client.Dispose();

    private static Uri Url(string path) => new(path, UriKind.Relative);

    private async Task<Guid> CreateCubeAsync()
    {
        var response = await _client.PostAsync(Url("/api/cubes"), content: null);
        var cube = await response.Content.ReadFromJsonAsync<CubeSnapshot>(Json);
        return cube!.Id;
    }

    private async Task RotateAsync(Guid id, Face face, Rotation rotation)
    {
        var response = await _client.PostAsJsonAsync(Url($"/api/cubes/{id}/rotations"), new RotateRequest(face, rotation), Json);
        response.EnsureSuccessStatusCode();
    }

    private static List<Colour> Colours(string facelets) => facelets.Select(ColourExtensions.FromSymbol).ToList();
}
