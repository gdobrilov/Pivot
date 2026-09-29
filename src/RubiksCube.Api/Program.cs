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

// Enums travel as names only ("Front", "Clockwise"); numbers are rejected. [ApiController] turns bad input into 400.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));

builder.Services.AddHealthChecks().AddDbContextCheck<RubiksDbContext>();

var app = builder.Build();

// Migrate at startup so a fresh checkout just runs. With several instances this would move to the deploy pipeline.
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<RubiksDbContext>().Database.MigrateAsync();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("ApiDocs:Enabled"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Pivot API"));
}

app.MapHealthChecks("/health");
app.MapControllers();

await app.RunAsync();

/// <summary>For WebApplicationFactory in the tests.</summary>
public partial class Program;
