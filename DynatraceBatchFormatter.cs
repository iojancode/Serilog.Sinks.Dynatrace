using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Serilog.Sinks.Http;

namespace Serilog.Sinks.Dynatrace
{
    class DynatraceBatchFormatter : IBatchFormatter
    {
        private readonly long? eventBodyLimitBytes;

        public DynatraceBatchFormatter(long? eventBodyLimitBytes = 256 * 1024)
        {
            this.eventBodyLimitBytes = eventBodyLimitBytes;
        }

        public void Format(IEnumerable<string> logEvents, TextWriter output)
        {
            if (logEvents == null) throw new ArgumentNullException(nameof(logEvents));
            if (output == null) throw new ArgumentNullException(nameof(output));

            var delimStart = "[";
            var any = false;

            foreach (var logEvent in logEvents)
            {
                if (string.IsNullOrWhiteSpace(logEvent)) continue; 

                if (Encoding.UTF8.GetByteCount(logEvent) <= eventBodyLimitBytes)
                {
                    output.Write(delimStart);
                    output.Write(logEvent);
                    delimStart = ",";
                    any = true;
                }
            }

            if (any)
            {
                output.Write("]");
            }
        }
    }
}