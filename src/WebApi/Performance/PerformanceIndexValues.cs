using MeasurementHelpers.Performance;

namespace WebApi.Performance
{
    public class PerformanceIndexValues : IPerformanceIndexValues
    {
        public double GetExpectedPerformanceSeconds(string context, string name)
        {
            return 0.2;
        }
    }
}