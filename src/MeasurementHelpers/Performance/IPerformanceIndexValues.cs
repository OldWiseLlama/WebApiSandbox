namespace MeasurementHelpers.Performance
{
    public interface IPerformanceIndexValues
    {
        double GetExpectedPerformanceSeconds(string context, string name);
    }
}