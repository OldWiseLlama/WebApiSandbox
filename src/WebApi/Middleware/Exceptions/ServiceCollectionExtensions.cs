using System;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Middleware.Exceptions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExceptionHandlingMiddleware(this IServiceCollection services, Action<ExceptionHandlingMiddlewareOptions> configure)
        {
            var builder = new ExceptionHandlingMiddlewareBuilder();
            configure?.Invoke(builder);
            
            builder.AddServices(services);

            services.AddSingleton(typeof(ExceptionHandlingMiddleware));
            
            return services;
        }
    }
}