using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class MeasureCallCountRenderer : MeasurementRenderer
    {
        public MeasureCallCountRenderer(int measurementIndex, int baseIndent, AttributeSyntax attribute, string classContext, string methodName) 
            : base(measurementIndex, baseIndent, attribute, classContext, methodName)
        {
        }

        public override IEnumerable<string> GetNamespaces() => new[] {"App.Metrics.Counter"};

        protected override bool HasInitialize => false;
        protected override bool HasFinalize => true;

        protected override void RenderFinalizeContent(StringBuilder builder, int indent)
        {
            RenderLine(builder, indent, "_metrics.Measure.Counter.Increment(new CounterOptions");
            RenderLine(builder, indent, '{');
            RenderFormatLine(builder, indent + 1, "Context = \"{0}\",", MeasurementContext);
            RenderFormatLine(builder, indent + 1, "Name = \"{0}\",", MeasurementName);
            RenderLine(builder, indent + 1, "MeasurementUnit = Unit.Calls");
            RenderLine(builder, indent, "});");
        }
    }
}