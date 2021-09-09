using System;
using Serilog.Core;
using Serilog.Events;
using AppContext = WebApi.AppContext;

namespace WebApi.Enrichers
{
    public class AmbientContextEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var correlationId = AppContext.GetCorrelationId();
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("correlationId", correlationId));
        }
    }
}