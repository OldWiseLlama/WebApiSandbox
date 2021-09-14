using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class PerformanceIndexRenderer : MeasurementRenderer
    {
        public PerformanceIndexRenderer(int measurementIndex, int baseIndent, AttributeSyntax attribute, string classContext, string methodName) : base(measurementIndex, baseIndent, attribute, classContext, methodName)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            return new[] {"App.Metrics.Apdex", "MeasurementHelpers.Performance"};
        }

        protected override bool HasInitialize => true;
        protected override bool HasFinalize => true;

        public override IEnumerable<DependencyInfo> GetDependencies()
        {
            return new[] {new DependencyInfo("IPerformanceIndexValues", "performanceIndexValues")};
        }

        protected override void RenderDeclareContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "ApdexContext tracker_{0} = default;", MeasurementIndex);
        }

        protected override void RenderInitializeContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "tracker_{0} = _metrics.Measure.Apdex.Track(new ApdexOptions", MeasurementIndex);
            RenderLine(builder, indent, '{');
            RenderFormatLine(builder, indent + 1, "Context = \"{0}\",", MeasurementContext);
            RenderFormatLine(builder, indent + 1, "Name = \"{0}\",", MeasurementName);
            RenderFormatLine(builder, indent + 1, "ApdexTSeconds = _performanceIndexValues.GetExpectedPerformanceSeconds(\"{0}\", \"{1}\")", MeasurementContext, MeasurementName);
            RenderLine(builder, indent, "});");
        }

        protected override void RenderFinalizeContent(StringBuilder builder, int indent)
        {
            RenderFormatLine(builder, indent, "tracker_{0}.Dispose();", MeasurementIndex);
        }
    }
}