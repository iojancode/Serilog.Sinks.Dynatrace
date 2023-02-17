using System;
using System.Collections.Generic;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Dynatrace;

namespace Serilog
{
    public static class DynatraceExtensions
    {
        /// <summary>
        /// Adds a Sink that logs to Dynatrace via the ingest endpoint
        /// </summary>
        /// <param name="sinkConfiguration"></param>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="batchPostingLimit">Serilog http sink pass-through</param>
        /// <param name="queueLimit">Serilog http sink pass-through</param>
        /// <param name="period">Serilog http sink pass-through</param>
        /// <param name="restrictedToMinimumLevel">Serilog http sink pass-through</param>
        /// <param name="propertiesPrefix">A prefix for properties derived from the serilog log template arguments</param>
        /// <param name="staticAttributes">Additional metadata akin to the application.id that will be set on each log message</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration Dynatrace(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            int? batchPostingLimit = null,
            int? queueLimit = null,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string propertiesPrefix = "attr.",
            Dictionary<string,string> staticAttributes = null)
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
                batchPostingLimit: batchPostingLimit ?? 50,
                queueLimit: queueLimit ?? 100,
                period: period ?? TimeSpan.FromSeconds(15),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix, staticAttributes),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }

        /// <summary>
        /// Adds a Sink that logs to Dynatrace via the ingest endpoint with DurableHttp
        /// </summary>
        /// <param name="sinkConfiguration"></param>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="batchPostingLimit">Serilog http sink pass-through</param>
        /// <param name="bufferPathFormat">Serilog http sink pass-through</param>
        /// <param name="period">Serilog http sink pass-through</param>
        /// <param name="restrictedToMinimumLevel">Serilog http sink pass-through</param>
        /// <param name="propertiesPrefix">A prefix for properties derived from the serilog log template arguments</param>
        /// <param name="staticAttributes">Additional metadata akin to application.id that will be set on each log message</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static LoggerConfiguration DurableDynatrace(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            int? batchPostingLimit = null,
            string bufferPathFormat = "dynatrace-buffer-{Date}.json",
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string propertiesPrefix = "attr.",
            Dictionary<string,string> staticAttributes = null)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.DurableHttp(ingestUrl,
                bufferPathFormat: bufferPathFormat,
                batchPostingLimit: batchPostingLimit ?? 50,
                period: period ?? TimeSpan.FromSeconds(15),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix, staticAttributes),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }
    }
}
