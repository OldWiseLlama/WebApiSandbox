using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middleware.Exceptions
{
    public abstract class ProblemDetailsMapperBase : IProblemDetailsMapper
    {
        public abstract Type ExceptionType { get; }
        
        public virtual bool CanHandle(Exception exception)
        {
            return ExceptionType.IsInstanceOfType(exception);
        }

        public abstract ProblemDetails MapToProblemDetails(Exception exception);
    }
}