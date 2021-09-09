using System;

namespace MeasurementHelpers.Attributes.Measurements
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MeasureCallCountAttribute : Attribute
    {
        public MeasureCallCountAttribute(string context = null, string name = null)
        {
            
        }
    }
}