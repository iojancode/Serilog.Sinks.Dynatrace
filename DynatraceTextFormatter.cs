using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.Dynatrace
{
    class DynatraceTextFormatter : ITextFormatter
    {
        private static readonly JsonValueFormatter Instance = new JsonValueFormatter();
        private string _applicationId;
        private string _hostName;
        private string _env;

        public DynatraceTextFormatter(string applicationId, string hostName, string env)
        {
            _applicationId = applicationId;
            _hostName = hostName;
            _env = env;
        }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            try
            {
                var buffer = new StringWriter();
                FormatContent(logEvent, buffer);
                output.WriteLine(buffer.ToString());
            }
            catch (Exception e)
            {
                LogNonFormattableEvent(logEvent, e);
            }
        }

        private void FormatContent(LogEvent logEvent, TextWriter output)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));
            if (output == null) throw new ArgumentNullException(nameof(output));

            output.Write("{\"timestamp\":\"");
            output.Write(logEvent.Timestamp.ToString("o"));

            output.Write("\",\"level\":\"");
            output.Write(logEvent.Level);

            output.Write("\",\"application.id\":\"");
            output.Write(_applicationId);

            output.Write("\",\"host.name\":\"");
            output.Write(_hostName);

            if (_env != null) 
            {
                output.Write("\",\"env\":\"");
                output.Write(_env);                
            }

            output.Write("\",\"content\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            var exception = logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "";
            JsonValueFormatter.WriteQuotedJsonString(message + exception, output);

            if (logEvent.Properties.Count != 0)
            {
                WriteProperties(logEvent.Properties, output);
            }

            output.Write('}');
        }

        private static void WriteProperties(
            IReadOnlyDictionary<string, LogEventPropertyValue> properties,
            TextWriter output)
        {
            output.Write(",\"data\":{");

            var precedingDelimiter = "";

            foreach (var property in properties)
            {
                output.Write(precedingDelimiter);
                precedingDelimiter = ",";

                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                Instance.Format(property.Value, output);
            }

            output.Write('}');
        }

        private static void LogNonFormattableEvent(LogEvent logEvent, Exception e)
        {
            SelfLog.WriteLine(
                "Event at {0} with message template {1} could not be formatted into JSON and will be dropped: {2}",
                logEvent.Timestamp.ToString("o"),
                logEvent.MessageTemplate.Text,
                e);
        }
    }
}