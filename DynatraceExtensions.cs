using System;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Dynatrace;

namespace Serilog
{
    public static class DynatraceExtensions
    {
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
            string propertiesPrefix = "attr.")
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
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }

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
            string propertiesPrefix = "attr.")
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
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName, propertiesPrefix),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }
    }
}
