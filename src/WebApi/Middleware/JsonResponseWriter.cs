using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApi.Middleware
{
    public class JsonResponseWriter : IResponseWriter
    {
        public async Task WriteToResponseAsync<TContent>(HttpResponse response, TContent content, int statusCode,
            CancellationToken cancellationToken = default)
        {
            response.StatusCode = statusCode;
            await response.WriteAsJsonAsync(content, cancellationToken);
        }
    }
}