using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Currency.Application.Helpers.Observability
{
    // Adds TraceId/SpanId from Activity.Current to each log
    public sealed class ActivityTraceEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory pf)
        {
            var act = Activity.Current;
            if (act is null) return;

            logEvent.AddPropertyIfAbsent(pf.CreateProperty("TraceId", act.TraceId.ToString()));
            logEvent.AddPropertyIfAbsent(pf.CreateProperty("SpanId", act.SpanId.ToString()));
        }
    }
}
