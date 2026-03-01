using Exceptionless;
using Exceptionless.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// configure Exceptionless
builder.Services.AddExceptionless(c => c.ReadFromConfiguration(builder.Configuration));

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseExceptionless();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Test Endpoints
app.MapGet("/api/test/ping", () => Results.Ok(new { message = "pong", timestamp = DateTime.UtcNow }))
    .WithName("TestPing");

app.MapGet("/api/test/info", () => Results.Ok(new {
    name = "ERP Discoteca API",
    status = "running",
    version = "1.0.0"
}))
.WithName("TestInfo");

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
