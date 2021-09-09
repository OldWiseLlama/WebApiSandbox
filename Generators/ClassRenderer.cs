using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class ClassRenderer
    {
        private static readonly string[] CommonNamespaces =
        {
            "System",
            "System.Runtime.ExceptionServices",
            "App.Metrics",
            "System.Diagnostics",
            "Microsoft.Extensions.Logging"
        };

        private readonly string _implementedInterfaceName;
        private readonly string _generatedTypeName;
        private readonly List<MethodRenderer> _methodRenderers;
        private readonly int _baseIndent;

        public ClassRenderer(int baseIndent, TypeDeclarationSyntax decoratedType)
        {
            _baseIndent = baseIndent;
            _implementedInterfaceName = decoratedType.Identifier.ValueText;
            _generatedTypeName = GetGeneratedTypeName(decoratedType);
            var classContext = GetClassContext(decoratedType, _implementedInterfaceName);
            _methodRenderers = decoratedType.Members.OfType<MethodDeclarationSyntax>()
                .Select(m => new MethodRenderer(2, classContext, m)).ToList();
        }

        public string GetGeneratedTypeName()
        {
            return _generatedTypeName;
        }
        
        public IEnumerable<string> GetNamespaces()
        {
            return CommonNamespaces.Union(_methodRenderers.SelectMany(r => r.GetNamespaces())).Distinct();
        }

        public void Render(StringBuilder builder)
        {
            RenderFormatLine(builder, _baseIndent, "public sealed class {0} : {1}", _generatedTypeName,
                _implementedInterfaceName);
            RenderLine(builder, _baseIndent, '{');
            RenderBody(builder);
            RenderLine(builder, _baseIndent, '}');
        }

        private void RenderBody(StringBuilder builder)
        {
            var indent = _baseIndent + 1;
            RenderLine(builder, indent, "private readonly IMetrics _metrics;");
            RenderFormatLine(builder, indent, "private readonly ILogger<{0}> _logger;", _generatedTypeName);
            RenderFormatLine(builder, indent, "private readonly {0} _decorated;", _implementedInterfaceName);
            RenderDependencyFields(builder);
            builder.AppendLine();
            RenderConstructor(builder);
            RenderMethods(builder);
        }

        private void RenderDependencyFields(StringBuilder builder)
        {
            var indent = _baseIndent + 1;
            foreach (var dependencyInfo in GetDependencyInfos())
            {
                RenderFormatLine(builder, indent, "private readonly {0} _{1};", dependencyInfo.TypeName, dependencyInfo.Name);
            }
        }

        private IEnumerable<DependencyInfo> GetDependencyInfos()
        {
            return _methodRenderers.SelectMany(m => m.GetDependencies()).Distinct();
        }

        private void RenderMethods(StringBuilder builder)
        {
            foreach (var methodRenderer in _methodRenderers)
            {
                builder.AppendLine();
                methodRenderer.Render(builder);
            }
        }

        private void RenderConstructor(StringBuilder builder)
        {
            var indent = _baseIndent + 1;
            RenderFormat(builder, indent, "public {0}(ILogger<{0}> logger, {1} decorated, IMetrics metrics", _generatedTypeName, _implementedInterfaceName);
            foreach (var dependencyInfo in GetDependencyInfos())
            {
                builder.Append(", ");
                builder.AppendFormat("{0} {1}", dependencyInfo.TypeName, dependencyInfo.Name);
            }
            builder.Append(')');
            builder.AppendLine();
            
            RenderLine(builder, indent, '{');
            RenderLine(builder, indent+1, "_logger = logger;");
            RenderLine(builder, indent+1, "_decorated = decorated;");
            RenderLine(builder, indent+1, "_metrics = metrics;");
            foreach (var dependencyInfo in GetDependencyInfos())
            {
                RenderFormatLine(builder, indent+1, "_{0} = {0};", dependencyInfo.Name);
            }
            RenderLine(builder, indent, '}');
        }

        private static string GetClassContext(TypeDeclarationSyntax type, string implementedInterfaceName)
        {
            var gatherMetricsAttribute = ResolveAttribute(type, name => name == "GatherMetrics");
            var attributeContext =
                gatherMetricsAttribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression.GetFirstToken().ValueText;

            var metricsContext = attributeContext ?? implementedInterfaceName;
            return metricsContext;
        }
        
        private static AttributeSyntax ResolveAttribute(TypeDeclarationSyntax type, Func<string, bool> nameSelector)
        {
            return ResolveAttribute(type.AttributeLists, nameSelector);
        }

        private static AttributeSyntax ResolveAttribute(SyntaxList<AttributeListSyntax> attributeLists,
            Func<string, bool> nameSelector)
        {
            var gatherMetricsAttribute = attributeLists
                .Select(a => a.Attributes.First(at => nameSelector(at.Name.GetText().ToString()))).First();
            return gatherMetricsAttribute;
        }
        
        private static string GetGeneratedTypeName(TypeDeclarationSyntax type)
        {
            return type.Identifier.ValueText.StartsWith("I")
                ? $"Generated{type.Identifier.ValueText.Substring(1)}MetricsDecorator"
                : $"Generated{type.Identifier.ValueText}MetricsDecorator";
        }
    }
}