using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebApi.Middleware.Exceptions
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IResponseWriter _responseWriter;
        private readonly IReadOnlyDictionary<Type, IProblemDetailsMapper> _problemDetailsMappers;

        public ExceptionHandlingMiddleware(
            ILogger<ExceptionHandlingMiddleware> logger,
            IEnumerable<IProblemDetailsMapper> problemDetailsMappers,
            IResponseWriter responseWriter)
        {
            _logger = logger;
            _responseWriter = responseWriter;
            _problemDetailsMappers = problemDetailsMappers.ToDictionary(m => m.ExceptionType);
        }

        public async Task InvokeAsync(HttpContext httpContext, RequestDelegate next)
        {
            try
            {
                await next(httpContext);
            }
            catch (TaskCanceledException e)
            {
                httpContext.Response.Clear();
            }
            catch (Exception exception)
            {
                if (httpContext.Response.HasStarted)
                {
                    // Cannot handle if the response has already been started.
                    _logger.LogWarning(exception, "Cannot handle exception because the response has already been started");
                    throw;
                }
                
                await HandleExceptionAsync(httpContext, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var mapper = ResolveMapper(exception);
            if (mapper is null)
            {
                _logger.LogWarning(exception, "Cannot handle exception because exception does not have problem details mapper configured");
                ExceptionDispatchInfo.Throw(exception);
            }

            _logger.LogError(exception, "Caught exception from {path}", httpContext.Request.Path);

            var problemDetails = mapper.MapToProblemDetails(exception);
            await _responseWriter.WriteToResponseAsync(httpContext.Response, problemDetails, problemDetails.Status ?? 500, httpContext.RequestAborted);
        }

        private IProblemDetailsMapper ResolveMapper(Exception exception)
        {
            var exceptionType = exception.GetType();
            IProblemDetailsMapper mapper;
            while ((!_problemDetailsMappers.TryGetValue(exceptionType!, out mapper) || !mapper.CanHandle(exception)) && exceptionType.BaseType != null )
            {
                exceptionType = exceptionType.BaseType;
            }

            return mapper;
        }
    }
}