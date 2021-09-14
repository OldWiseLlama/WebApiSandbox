using System;

namespace MeasurementHelpers.Attributes.Measurements
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class PerformanceIndexAttribute : Attribute
    {
        public PerformanceIndexAttribute(string context = null, string name = null)
        {
            
        }
    }
}