using System;
using System.Threading;
using System.Threading.Tasks;
using MeasurementHelpers.Attributes;
using MeasurementHelpers.Attributes.Measurements;

namespace WebApi.Services
{
    [GatherMetrics("CustomContext")]
    public interface IServiceInterface
    {
        [MeasureDuration(name:"TheOperation", context:"WrongOrderContext"), MeasureCallCount, MeasureHitRate("RateContext", "Perform"), PerformanceIndex("Performance", "Perform")]
        Task PerformAsync(TimeSpan waitTime, CancellationToken? cancellationToken = null);

        [MeasureDuration]
        [MeasureCallCount()]
        void DoSync();

        [MeasureDuration(context:"OtherContext")]
        string GetValue();
        
        [MeasureCallCount("DifferentContext", "DifferentName")]
        Task<int> GetNumberAsync();
    }
}