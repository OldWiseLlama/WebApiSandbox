using System;

namespace MeasurementHelpers.Attributes.Measurements
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MeasureHitRateAttribute : Attribute
    {
        public MeasureHitRateAttribute(string context = null, string name = null)
        {
            
        }
    }
}