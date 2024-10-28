using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Configure Logging
builder.Logging.ClearProviders();

builder.Logging.AddSerilog(Log.Logger, dispose: true);

// Define some constants for the application
var serviceName = "WeatherService";
var serviceVersion = "1.0.0";

// Create meter
var meter = new Meter(serviceName, serviceVersion);
var requestCounter = meter.CreateCounter<int>("weather_requests", description: "Number of weather requests");

// Create ActivitySource for tracing
var activitySource = new ActivitySource(serviceName);

// Configure OpenTelemetry Resource
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = "development",
        ["host.name"] = Environment.MachineName
    });

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing
            .SetResourceBuilder(resourceBuilder)
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation(opts =>
            {
                opts.RecordException = true;
                opts.EnrichWithHttpRequest = (activity, request) =>
                {
                    activity.SetTag("custom.request.path", request.Path);
                };
            })
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri("http://localhost:4317");
            });
    })
    .WithMetrics(metrics =>
    {
        metrics
            .SetResourceBuilder(resourceBuilder)
            .AddMeter(serviceName)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(opts =>
            {
                opts.Endpoint = new Uri("http://localhost:4317");
            });
    });

builder.Logging.AddOpenTelemetry(options =>
{
    options.SetResourceBuilder(resourceBuilder);
    options.IncludeFormattedMessage = true;
    options.IncludeScopes = true;
    options.ParseStateValues = true;
    options.AddConsoleExporter();
    options.AddOtlpExporter(opts =>
    {
        opts.Endpoint = new Uri("http://localhost:4317");
    });
});

var app = builder.Build();

// Add a middleware to ensure we capture all requests
app.Use(async (context, next) =>
{
    using var activity = activitySource.StartActivity("HttpRequest");
    activity?.SetTag("http.path", context.Request.Path);
    await next();
});

// Sample endpoints that generate telemetry
app.MapGet("/weather", async (ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("GetWeather");
    activity?.SetTag("weather.type", "current");
    
    logger.LogInformation("Processing weather request at {Time}", DateTimeOffset.UtcNow);
    
    try
    {
        // Simulate some work with a child span
        using (var workActivity = activitySource.StartActivity("ProcessWeather"))
        {
            logger.LogInformation("Fetching current weather data");
            await Task.Delay(100);
            workActivity?.SetTag("processing.type", "current_weather");
        }
        
        // Increment the counter
        requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/weather"));
        
        logger.LogInformation("Successfully processed weather request");
        
        return new WeatherForecast
        {
            Date = DateTimeOffset.Now,
            Temperature = 20,
            Summary = "Sunny"
        };
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing weather request");
        throw;
    }
});

app.MapGet("/weather/forecast", async (ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("GetWeatherForecast");
    activity?.SetTag("weather.type", "forecast");
    
    logger.LogInformation("Processing forecast request at {Time}", DateTimeOffset.UtcNow);
    
    try
    {
        // Simulate database call
        using (var dbActivity = activitySource.StartActivity("DatabaseQuery"))
        {
            logger.LogInformation("Querying weather forecast database");
            await Task.Delay(150);
            dbActivity?.SetTag("database.query", "SELECT * FROM forecasts");
        }
        
        // Increment the counter
        requestCounter.Add(1, new KeyValuePair<string, object?>("endpoint", "/weather/forecast"));
        
        logger.LogInformation("Successfully processed forecast request");
        
        return new WeatherForecast
        {
            Date = DateTimeOffset.Now.AddDays(1),
            Temperature = 25,
            Summary = "Sunny and warm"
        };
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing forecast request");
        throw;
    }
});

app.MapGet("/weather/error", (ILogger<Program> logger) =>
{
    using var activity = activitySource.StartActivity("GetWeatherError");
    logger.LogWarning("Accessing error-generating endpoint");
    
    activity?.SetTag("weather.type", "error");
    logger.LogError("Simulated error in weather processing");
    
    throw new Exception("Simulated weather error");
});

app.Run();

public class WeatherForecast
{
    public DateTimeOffset Date { get; set; }
    public int Temperature { get; set; }
    public string? Summary { get; set; }
}