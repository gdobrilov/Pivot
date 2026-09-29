using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
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

// Enums travel as their names ("Front", "Clockwise"), which is what a UI shows and what a reviewer can read.
// [ApiController] turns model-binding failures (unknown face name, malformed JSON) into 400 problem details.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddHealthChecks().AddDbContextCheck<RubiksDbContext>();

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

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentCorsPolicy);
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Rubik's Cube API"));
}

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

/// <summary>Exposed so integration tests can host the API in-process with WebApplicationFactory.</summary>
public partial class Program;
