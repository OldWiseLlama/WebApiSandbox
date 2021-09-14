using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.Services
{
    public class AwesomeService : IServiceInterface
    {
        public async Task PerformAsync(TimeSpan waitTime, CancellationToken? cancellationToken = null)
        {
            await Task.Delay(waitTime, cancellationToken ?? CancellationToken.None);
        }

        public void DoSync()
        {
        }

        public string GetValue()
        {
            return "Foo";
        }

        public async Task<int> GetNumberAsync()
        {
            await Task.Delay(50);
            return 40;
        }
    }
}