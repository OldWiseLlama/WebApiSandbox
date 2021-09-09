using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Timer;

namespace WebApi.Services
{
    public class ReplacingService : IServiceInterface
    {
        public Task PerformAsync(TimeSpan waitTime, CancellationToken? cancellationToken = null)
        {
            IMetrics metrics = null;
            var time = metrics.Measure.Timer.Time(new TimerOptions());
            
            time.Dispose();
            throw new System.NotImplementedException();
        }

        public void DoSync()
        {
            throw new System.NotImplementedException();
        }

        public string GetValue()
        {
            throw new System.NotImplementedException();
        }

        public Task<int> GetNumberAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}