using Exceptionless;
using Exceptionless.Models;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// configure Exceptionless
builder.Services.AddExceptionless(c => c.ReadFromConfiguration(builder.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Servers = [new OpenApiServer { Url = "https://api.saidvox.dev" }];
        return Task.CompletedTask;
    });
});
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();

app.UseExceptionless();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Development specific features can go here
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.MapOpenApi();
app.MapScalarApiReference();

// Test Endpoints
app.MapGet("/api/test/ping", () => Results.Ok(new { message = "pong", timestamp = DateTime.UtcNow }))
    .WithName("TestPing");

app.MapGet("/api/test/info", () => Results.Ok(new {
    name = "ERP Discoteca API",
    status = "running",
    version = "1.0.0"
}))
.WithName("TestInfo");

// Nuevo endpoint interactivo de prueba
app.MapGet("/api/test/random", () => Results.Ok(new {
    id = Guid.NewGuid(),
    value = Random.Shared.Next(1, 1000),
    timestamp = DateTime.UtcNow
}))
.WithName("TestRandom");

// Endpoint de estado de salud (Health Check)
app.MapGet("/api/test/health", () => Results.Ok(new {
    status = "healthy",
    checks = new[] { "database", "cache", "external_api" },
    timestamp = DateTime.UtcNow
}))
.WithName("TestHealth");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILogger<Program> logger) =>
{
    // TODO: Fix ExceptionlessState reference
    // using var _ = logger.BeginScope(new ExceptionlessState()
    //    .Tag("Weather")
    //    .Property("SummariesCount", summaries.Length));

    using var _ = logger.BeginScope(new Dictionary<string, object>
    {
        ["Tag"] = "Weather",
        ["SummariesCount"] = summaries.Length
    });

    logger.LogInformation("Getting weather forecast");

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
