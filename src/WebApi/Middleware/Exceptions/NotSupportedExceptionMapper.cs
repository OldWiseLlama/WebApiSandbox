using System;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Middleware.Exceptions
{
    public class NotSupportedExceptionMapper : ProblemDetailsMapperBase
    {
        public override Type ExceptionType => typeof(NotSupportedException);
        public override ProblemDetails MapToProblemDetails(Exception exception)
        {
            return new ProblemDetails
            {
                Detail = "The operation is not supported.",
                Title = "Not supported",
                Status = 404,
                Type = "foo"
            };
        }
    }
}