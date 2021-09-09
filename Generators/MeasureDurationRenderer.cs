using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class MeasureDurationRenderer : MeasurementRenderer
    {
        public MeasureDurationRenderer(int measurementIndex, int baseIndent, AttributeSyntax attribute, string classContext, string methodName) 
            : base(measurementIndex, baseIndent, attribute, classContext, methodName)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            return new[] {"App.Metrics.Timer"};
        }

        protected override bool HasInitialize => true;
        protected override bool HasFinalize => true;

        protected override void RenderDeclareContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "TimerContext timer_{0} = default;", MeasurementIndex);
        }

        protected override void RenderInitializeContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "timer_{0} = _metrics.Measure.Timer.Time(new TimerOptions", MeasurementIndex);
            RenderLine(builder, indent, '{');
            RenderFormatLine(builder, indent + 1, "Context = \"{0}\",", MeasurementContext);
            RenderFormatLine(builder, indent + 1, "Name = \"{0}\",", MeasurementName);
            RenderLine(builder, indent + 1, "DurationUnit = TimeUnit.Nanoseconds");
            RenderLine(builder, indent, "});");
            
        }

        protected override void RenderFinalizeContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "timer_{0}.Dispose();", MeasurementIndex);
        }
    }
}