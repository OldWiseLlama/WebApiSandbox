using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement.Mvc;

namespace WebApi.FeatureHelpers
{
    public class DisabledFeatureHandlers : IDisabledFeaturesHandler
    {
        public Task HandleDisabledFeatures(IEnumerable<string> features, ActionExecutingContext context)
        {
            context.Result = new ObjectResult(new ProblemDetails
            {
                Detail = "This endpoint is currently not supported",
                Status = 404
            });
            context.HttpContext.Response.StatusCode = 404;

            return Task.CompletedTask;
        }
    }
}