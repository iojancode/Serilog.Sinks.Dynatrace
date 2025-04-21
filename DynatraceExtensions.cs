using System;
using System.Net;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Dynatrace;
using Serilog.Sinks.Http;

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
        /// <param name="queueLimitBytes">The maximum size, in bytes, of events stored in memory, waiting to be sent over the network. Specify null for no limit</param>
        /// <param name="logEventsInBatchLimit">The maximum number of log events sent as a single batch over the network. Default value is 1000.</param>
        /// <param name="period">The time to wait between checking for event batches. Default value is 5 seconds.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink. Default value is Serilog.Events.LevelAlias.Minimum.</param>
        /// <returns>Serilog LoggerConfiguration</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken or ingestUrl is null</exception>
        public static LoggerConfiguration Dynatrace(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            long? queueLimitBytes = null,
            int? logEventsInBatchLimit = 1000,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.Http(
                requestUri: ingestUrl,
                queueLimitBytes: queueLimitBytes,
                logEventsInBatchLimit: logEventsInBatchLimit,
                batchSizeLimitBytes: 5 * ByteSize.MB, // max 10MB per request, extra characters for json not included 
                period: period ?? TimeSpan.FromSeconds(5),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }

        /// <summary>
        /// Adds a durable Sink that logs to Dynatrace via the ingest endpoint. The log events are always stored on disk in the case that the log server cannot be reached.
        /// </summary>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="bufferBaseFileName">The relative or absolute path for a set of files that will be used to buffer events until they can be successfully transmitted across the network. Individual files will be created using the pattern "bufferBaseFileName-*.txt", which should not clash with any other file names in the same directory. Default value is "Buffer".</param>
        /// <param name="bufferFileSizeLimitBytes">The approximate maximum size, in bytes, to which a buffer file will be allowed to grow. For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial events, the last event within the limit will be written in full even if it exceeds the limit.</param>
        /// <param name="bufferFileShared">Allow the buffer file to be shared by multiple processes. Default value is false.</param>
        /// <param name="retainedBufferFileCountLimit">The maximum number of buffer files that will be retained, including the current buffer file. Under normal operation only 2 files will be kept, however if the log server is unreachable, the number of files specified by retainedBufferFileCountLimit will be kept on the file system. For unlimited retention, pass null. Default value is 31.</param>
        /// <param name="logEventsInBatchLimit">The maximum number of log events sent as a single batch over the network. Default value is 1000.</param>
        /// <param name="period">The time to wait between checking for event batches. Default value is 5 seconds.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink. Default value is Serilog.Events.LevelAlias.Minimum.</param>
        /// <returns>Serilog LoggerConfiguration</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken or ingestUrl is null</exception>
        public static LoggerConfiguration DurableDynatraceUsingFileSizeRolledBuffers(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            string bufferBaseFileName = "dynatrace-buffer",
            long? bufferFileSizeLimitBytes = ByteSize.GB,
            bool bufferFileShared = false,
            int? retainedBufferFileCountLimit = 31,
            int? logEventsInBatchLimit = 1000,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.DurableHttpUsingFileSizeRolledBuffers(
                requestUri: ingestUrl,
                bufferBaseFileName: bufferBaseFileName,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: bufferFileShared,
                retainedBufferFileCountLimit: retainedBufferFileCountLimit,
                logEventsInBatchLimit: logEventsInBatchLimit,
                batchSizeLimitBytes: 5 * ByteSize.MB, // max 10MB per request, extra characters for json not included 
                period: period ?? TimeSpan.FromSeconds(5),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }

        /// <summary>
        /// Adds a durable Sink that logs to Dynatrace via the ingest endpoint. The log events are always stored on disk in the case that the log server cannot be reached.
        /// </summary>
        /// <param name="accessToken">The Dynatrace ApiToken</param>
        /// <param name="ingestUrl">The endpoint for log ingestion, usually of the form "https://{instanceId}.live.dynatrace.com/api/v2/logs/ingest"</param>
        /// <param name="applicationId">Will be populated as application.id in all log entries</param>
        /// <param name="hostName">Will be populated as host.name in all log entries</param>
        /// <param name="bufferBaseFileName">The relative or absolute path for a set of files that will be used to buffer events until they can be successfully transmitted across the network. Individual files will be created using the pattern "bufferBaseFileName-*.txt", which should not clash with any other file names in the same directory. Default value is "Buffer".</param>
        /// <param name="bufferFileSizeLimitBytes">The approximate maximum size, in bytes, to which a buffer file for a specific time interval will be allowed to grow. By default no limit will be applied</param>
        /// <param name="bufferFileShared">Allow the buffer file to be shared by multiple processes. Default value is false.</param>
        /// <param name="retainedBufferFileCountLimit">The maximum number of buffer files that will be retained, including the current buffer file. Under normal operation only 2 files will be kept, however if the log server is unreachable, the number of files specified by retainedBufferFileCountLimit will be kept on the file system. For unlimited retention, pass null. Default value is 31.</param>
        /// <param name="logEventsInBatchLimit">The maximum number of log events sent as a single batch over the network. Default value is 1000.</param>
        /// <param name="period">The time to wait between checking for event batches. Default value is 5 seconds.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for events passed through the sink. Default value is Serilog.Events.LevelAlias.Minimum.</param>
        /// <returns>Serilog LoggerConfiguration</returns>
        /// <exception cref="ArgumentNullException">Thrown when accessToken or ingestUrl is null</exception>
        public static LoggerConfiguration DurableDynatraceUsingTimeRolledBuffers(
            this LoggerSinkConfiguration sinkConfiguration,
            string accessToken,
            string ingestUrl,
            string applicationId = null,
            string hostName = null,
            string bufferBaseFileName = "dynatrace-buffer",
            BufferRollingInterval bufferRollingInterval = BufferRollingInterval.Day,
            long? bufferFileSizeLimitBytes = null,
            bool bufferFileShared = false,
            int? retainedBufferFileCountLimit = 31,
            int? logEventsInBatchLimit = 1000,
            TimeSpan? period = null,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum)
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentNullException(nameof(accessToken));
            if (string.IsNullOrWhiteSpace(ingestUrl)) throw new ArgumentNullException(nameof(ingestUrl));

            if (applicationId == null) applicationId = "unknown";
            if (hostName == null) hostName = Dns.GetHostName().ToLower();

            var envName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNET_ENVIRONMENT");

            return sinkConfiguration.DurableHttpUsingTimeRolledBuffers(
                requestUri: ingestUrl,
                bufferBaseFileName: bufferBaseFileName,
                bufferRollingInterval: bufferRollingInterval,
                bufferFileSizeLimitBytes: bufferFileSizeLimitBytes,
                bufferFileShared: bufferFileShared,
                retainedBufferFileCountLimit: retainedBufferFileCountLimit,
                logEventsInBatchLimit: logEventsInBatchLimit,
                batchSizeLimitBytes: 5 * ByteSize.MB, // max 10MB per request, extra characters for json not included 
                period: period ?? TimeSpan.FromSeconds(5),
                textFormatter: new DynatraceTextFormatter(applicationId, hostName, envName),
                batchFormatter: new DynatraceBatchFormatter(),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                httpClient: new DynatraceHttpClient(accessToken));
        }
    }
}
