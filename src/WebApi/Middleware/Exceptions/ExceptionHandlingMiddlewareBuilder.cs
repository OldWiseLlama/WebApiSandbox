using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace WebApi.Middleware.Exceptions
{
    public class ExceptionHandlingMiddlewareBuilder : ExceptionHandlingMiddlewareOptions
    {
        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton(typeof(IResponseWriter), ResponseWriterType);
            foreach (var (type, activator) in MapperTypes)
            {
                if (activator != null)
                {
                    services.AddSingleton(type, activator);
                }
                else
                {
                    services.AddSingleton(typeof(IProblemDetailsMapper), type);
                }
            }
            
        }
    }

    public abstract class ExceptionHandlingMiddlewareOptions
    {
        protected readonly List<(Type Type, Func<IServiceProvider, IProblemDetailsMapper> Activator)> MapperTypes = new();
        protected Type ResponseWriterType = typeof(JsonResponseWriter);

        public ExceptionHandlingMiddlewareOptions WithMapper<TMapper>() where TMapper : class, IProblemDetailsMapper
        {
            MapperTypes.Add((typeof(TMapper), null));
            return this;
        }

        public ExceptionHandlingMiddlewareOptions WithMapper<TMapper>(Func<IServiceProvider, TMapper> activator) where TMapper : class, IProblemDetailsMapper
        {
            MapperTypes.Add((typeof(TMapper), activator));
            return this;
        }

        public ExceptionHandlingMiddlewareOptions WithResponseWriter<TWriter>() where TWriter : class, IResponseWriter
        {
            ResponseWriterType = typeof(TWriter);
            return this;
        }
    }
}