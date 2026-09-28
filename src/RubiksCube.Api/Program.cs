using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RubiksCube.Api.Endpoints;
using RubiksCube.Application;
using RubiksCube.Infrastructure;
using RubiksCube.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Rubiks") ?? "Data Source=rubiks.db";

builder.Services
    .AddApplication()
    .AddInfrastructure(options => options.UseSqlite(connectionString))
    .AddProblemDetails()
    .AddOpenApi();

builder.Services.AddHealthChecks().AddDbContextCheck<RubiksDbContext>();

// Enums travel as their names ("Front", "Clockwise"), which is what a UI shows and what a reviewer can read.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// The React dev server (Vite) runs on another origin during local development.
const string DevelopmentCorsPolicy = "Development";
builder.Services.AddCors(options => options.AddPolicy(
    DevelopmentCorsPolicy,
    policy => policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Apply pending migrations so a fresh checkout needs no manual database step.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<RubiksDbContext>().Database.MigrateAsync();
}

// Malformed input (e.g. an unknown face name in the JSON body) is the client's fault, not ours:
// keep the 400 that model binding chose instead of turning it into a 500.
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = exception => exception is BadHttpRequestException bad ? bad.StatusCode : StatusCodes.Status500InternalServerError,
});
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Rubik's Cube API"));
}

app.MapHealthChecks("/health");
app.MapCubeEndpoints();

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API in-process with WebApplicationFactory.</summary>
public partial class Program;
