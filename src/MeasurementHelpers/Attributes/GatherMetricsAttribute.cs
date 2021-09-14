using System;

namespace MeasurementHelpers.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class GatherMetricsAttribute : Attribute
    {
        public GatherMetricsAttribute(string context = null)
        {
            
        }
    }
}