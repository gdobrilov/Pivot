using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using RubiksCube.Application;
using RubiksCube.Infrastructure;
using RubiksCube.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Rubiks")
    ?? throw new InvalidOperationException("Connection string 'Rubiks' is not configured.");

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

// Origins allowed to call the API from a browser, e.g. the Vite dev server. Empty means none.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Migrate at startup so a fresh checkout just runs.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<RubiksDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Pivot API"));
}

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

/// <summary>For WebApplicationFactory in the tests.</summary>
public partial class Program;
