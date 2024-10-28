using System.Globalization;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace WeatherService
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithUtcTimestamp(
            this LoggerEnrichmentConfiguration enrich)
        {
            ArgumentNullException.ThrowIfNull(enrich);
            return enrich.With<UtcTimestampEnricher>();
        }
    }

    // https://github.com/serilog/serilog/issues/1024
    // https://www.ctrlaltdan.com/2018/08/14/custom-serilog-enrichers/
    public class UtcTimestampEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            ArgumentNullException.ThrowIfNull(logEvent);
            ArgumentNullException.ThrowIfNull(propertyFactory);
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "UtcTimestamp",
                logEvent.Timestamp.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss.fffZ", CultureInfo.InvariantCulture)));
        }
    }
}