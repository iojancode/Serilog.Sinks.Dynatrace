using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;

namespace Serilog.Sinks.Dynatrace
{
    class DynatraceTextFormatter : ITextFormatter
    {
        // Needed by dynatrace as is. Should not be prefixed
        private static readonly string[] ROOT_PROPERTIES = { 
            // Trace specifics
            "trace_id", 
            "span_id",
            "trace_sampled",

            // Process specifics
            "dt.entity.process_group_instance",

            // Host specifics
            "dt.entity.host",
            "dt.host_group",
            "dt.host_group.id"}; 

        private readonly string _applicationId;
        private readonly string _hostName;
        private readonly string _environment;
        private readonly string _propertiesPrefix;
        private readonly IReadOnlyDictionary<string, string> _customAttributes;

        public DynatraceTextFormatter(string applicationId, string hostName, string environment, string propertiesPrefix, IReadOnlyDictionary<string, string> customAttributes)
        {
            _applicationId = applicationId;
            _hostName = hostName;
            _environment = environment;
            _propertiesPrefix = propertiesPrefix;
            _customAttributes = customAttributes;
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
            output.Write(logEvent.Timestamp.ToUnixTimeMilliseconds());

            output.Write("\",\"level\":\"");
            output.Write(logEvent.Level);

            output.Write("\",\"application.id\":\"");
            output.Write(_applicationId);

            output.Write("\",\"host.name\":\"");
            output.Write(_hostName);

            if (_environment != null) 
            {
                output.Write("\",\"environment\":\"");
                output.Write(_environment);                
            }

            output.Write("\",\"content\":");
            var message = logEvent.MessageTemplate.Render(logEvent.Properties);
            var exception = logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "";
            JsonValueFormatter.WriteQuotedJsonString(message + exception, output);

            if (logEvent.Properties.Count != 0)
            {
                WriteProperties(logEvent.Properties, output, _propertiesPrefix);
            }

            if (_customAttributes != null)
            {
                WriteAttributes(_customAttributes, output);
            }

            output.Write('}');
        }

        private static void WriteProperties(
            IReadOnlyDictionary<string, LogEventPropertyValue> properties,
            TextWriter output, string prefixKey)
        {
            foreach (var property in properties)
            {
                var flatKey = prefixKey + property.Key;
                if (ROOT_PROPERTIES.Contains(property.Key)) flatKey = property.Key; 
                switch (property.Value) 
                {
                    case ScalarValue scalar:
                        output.Write(",");
                        JsonValueFormatter.WriteQuotedJsonString(flatKey, output);
                        output.Write(':');
                        JsonValueFormatter.WriteQuotedJsonString(Convert.ToString(scalar.Value), output); // Only values of the String type are supported
                        break;
                    case SequenceValue sequence:
                        int seq = 0;
                        WriteProperties(sequence.Elements.ToDictionary(e => (seq++).ToString(), e => e), output, flatKey + ".");
                        break;
                    case StructureValue structure:
                        WriteProperties(structure.Properties.ToDictionary(p => p.Name, p => p.Value), output, flatKey + ".");
                        break;
                    case DictionaryValue dictionary:
                        WriteProperties(dictionary.Elements.ToDictionary(e => e.Key.Value.ToString(), e => e.Value), output, flatKey + ".");
                        break;
                }
            }
        }

        private static void WriteAttributes(
            IReadOnlyDictionary<string, string> attributes,
            TextWriter output)
        {
            foreach (var attributePair in attributes)
            {
                output.Write(",");
                JsonValueFormatter.WriteQuotedJsonString(attributePair.Key, output);
                output.Write(':');
                JsonValueFormatter.WriteQuotedJsonString(attributePair.Value, output);
            }
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