# Weather Service with OpenTelemetry

A sample ASP.NET Core application demonstrating the implementation of observability using OpenTelemetry. The application provides weather information through REST endpoints while collecting metrics, traces, and logs using industry-standard tools.

## Features

- **Metrics Collection**: Custom metrics and ASP.NET Core default metrics using OpenTelemetry
- **Distributed Tracing**: Request tracing with custom spans and attributes
- **Logging**: 
  - Structured logging to Loki via OpenTelemetry
  - File-based logging using Serilog
- **Observability Stack**:
  - Prometheus for metrics storage
  - Jaeger for distributed tracing
  - Loki for log aggregation
  - Grafana for visualization

## Getting Started

1. Clone the repository:

2. Start the observability stack:
```bash
docker-compose up -d
```

3. Run the application:
```bash
dotnet run
```

4. Hit API endpoints below to generate telemetry data:

## API Endpoints

- `GET /weather`: Returns current weather information
- `GET /weather/forecast`: Returns weather forecast
- `GET /weather/error`: Generates a sample error (for testing error tracking)

## Observability Components

### Metrics
- Prometheus: `http://localhost:9090`
- Custom metrics:
  - `weather_requests`: Counter of weather endpoint requests
- Default ASP.NET Core metrics included

### Traces
- Jaeger: `http://localhost:16686`
- Traces include:
  - HTTP request handling
  - Weather data processing
  - Database simulated calls
  - Error scenarios

### Logs
- OpenTelemetry logs available in Loki
- File-based logs using Serilog:
  - Location: `logs/weatherservice-.log`
  - JSON format: `logs/weatherservice-json-.log`
  - Rolling daily files with 7-day retention

### Visualization
- Grafana: `http://localhost:3000`
- Default credentials: admin/admin
- Preconfigured data sources:
  - Prometheus
  - Jaeger
  - Loki

## Development

### Adding Custom Metrics
```csharp
var meter = new Meter("ServiceName");
var counter = meter.CreateCounter<int>("metric_name");
counter.Add(1, new KeyValuePair<string, object?>("tag", "value"));
```

### Adding Custom Traces
```csharp
using var activity = activitySource.StartActivity("OperationName");
activity?.SetTag("custom.tag", "value");
```

### Adding Logs
```csharp
logger.LogInformation("Message with {Parameter}", value);
```

## References

[Simplify observability .NET with OpenTelemetry Collector](https://dev.to/kim-ch/observability-net-opentelemetry-collector-25g1)

[Example: Use OpenTelemetry with Prometheus, Grafana, and Jaeger](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-prgrja-example)

[What diagnostic tools are available in .NET Core?](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/)

https://opentelemetry.io/docs/zero-code/net/getting-started/


