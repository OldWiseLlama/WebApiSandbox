using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebApi.Middleware
{
    public interface IResponseWriter
    {
        Task WriteToResponseAsync<TContent>(HttpResponse response, TContent content, int statusCode,
            CancellationToken cancellationToken = default);
    }
}