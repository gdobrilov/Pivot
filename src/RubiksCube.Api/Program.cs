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

// Enums travel as names ("Front", "Clockwise"). [ApiController] turns bad input into 400 problem details.
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

// Migrate at startup so a fresh checkout just runs.
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

/// <summary>For WebApplicationFactory in the tests.</summary>
public partial class Program;
