using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApi.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string HeaderKey = "X-Awesome-CorrelationId";
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(
            ILogger<CorrelationIdMiddleware> logger,
            RequestDelegate next)
        {
            _logger = logger;
            _next = next;
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            if(!httpContext.Request.Headers.TryGetValue(HeaderKey, out var value)
                ||
                value.Count != 1
                ||
                !Guid.TryParse(value[0], out var correlationId))
            {
                correlationId = Guid.NewGuid();
                AppContext.SetCorrelationId(correlationId);
                _logger.LogTrace("Request does not specify correlation ID header {headerKey}, correlation ID generated {correlationId}", HeaderKey, correlationId);
            }
            
            await _next.Invoke(httpContext);
        }
    }
}