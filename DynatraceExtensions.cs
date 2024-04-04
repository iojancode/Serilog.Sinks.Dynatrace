using System;
using System.Collections.Generic;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Dynatrace;
using Serilog.Sinks.Http.BatchFormatters;

namespace Serilog
{
    public static class DynatraceExtensions
    {
        /// <summary>
        /// Adds a Sink that logs to Dynatrace via the ingest endpoint
        /// </summary>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="logEventsInBatchLimit">Serilog http sink pass-through</param>
        /// <param name="queueLimitBytes">Serilog http sink pass-through</param>
        /// <param name="period">Serilog http sink pass-through</param>
        /// <param name="restrictedToMinimumLevel">Serilog http sink pass-through</param>
        /// <param name="propertiesPrefix">A prefix for properties derived from the serilog log template arguments</param>
        /// <param name="customAttributes">Additional attributes that will be set on each log message</param>
        /// <returns>Serilog LoggerConfiguration</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken or ingestUrl is null</exception>
        public static LoggerConfiguration Dynatrace(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            int? logEventsInBatchLimit = null,
            int? queueLimitBytes = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string propertiesPrefix = "attr.",
            IReadOnlyDictionary<string, string> customAttributes = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.Http(ingestUrl,
                logEventsInBatchLimit: logEventsInBatchLimit ?? 50,
                queueLimitBytes: queueLimitBytes ?? 50,
                period: period ?? TimeSpan.FromSeconds(15),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix, customAttributes),
                batchFormatter: new ArrayBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }

        /// <summary>
        /// Adds a Sink that logs to Dynatrace via the ingest endpoint with DurableHttp
        /// </summary>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="logEventLimitBytes">Serilog http sink pass-through</param>        
        /// <param name="period">Serilog http sink pass-through</param>
        /// <param name="restrictedToMinimumLevel">Serilog http sink pass-through</param>
        /// <param name="propertiesPrefix">A prefix for properties derived from the serilog log template arguments</param>
        /// <param name="customAttributes">Additional attributes that will be set on each log message</param>
        /// <returns>Serilog LoggerConfiguration</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken or ingestUrl is null</exception>
        public static LoggerConfiguration DurableDynatrace(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            int? logEventLimitBytes = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string propertiesPrefix = "attr.",
            IReadOnlyDictionary<string, string> customAttributes = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(ingestUrl,                
                logEventLimitBytes: logEventLimitBytes ?? 50,
                period: period ?? TimeSpan.FromSeconds(15),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix, customAttributes),
                batchFormatter: new ArrayBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }
    }
}
