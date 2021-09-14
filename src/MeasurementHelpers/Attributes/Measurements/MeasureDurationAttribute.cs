using System;

namespace MeasurementHelpers.Attributes.Measurements
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MeasureDurationAttribute : Attribute
    {
        public MeasureDurationAttribute(string context = null, string name = null)
        {
            
        }
    }
}