using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middleware.Exceptions
{
    public interface IProblemDetailsMapper
    {
        Type ExceptionType { get; }
        
        bool CanHandle(Exception exception);
        
        ProblemDetails MapToProblemDetails(Exception exception);
    }
}