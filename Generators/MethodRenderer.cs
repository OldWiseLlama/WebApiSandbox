using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class MethodRenderer
    {
        private readonly int _baseIndent;
        private readonly MethodDeclarationSyntax _methodDeclaration;
        private readonly string _methodName;
        private readonly List<MeasurementRenderer> _measurementRenderers;

        public MethodRenderer(int baseIndent, string classContext, MethodDeclarationSyntax methodDeclaration)
        {
            _baseIndent = baseIndent;
            _methodDeclaration = methodDeclaration;
            _methodName = methodDeclaration.Identifier.ValueText;
            _measurementRenderers =
                MeasurementRenderer.Create(baseIndent + 1, _methodName, classContext, methodDeclaration).ToList();
        }

        public void Render(StringBuilder builder)
        {
            bool isAsync = IsMethodAsync();
            RenderSignature(builder, isAsync);
            RenderLine(builder, _baseIndent, '{');
            RenderBody(builder, isAsync);
            RenderLine(builder, _baseIndent, '}');
        }

        public IEnumerable<string> GetNamespaces()
        {
            return _measurementRenderers.SelectMany(r => r.GetNamespaces());
        }

        public IEnumerable<DependencyInfo> GetDependencies()
        {
            return _measurementRenderers.SelectMany(r => r.GetDependencies()).Distinct();
        }

        private void RenderBody(StringBuilder builder, bool isAsync)
        {
            bool returnsValue = ReturnsValue();
            var indent = _baseIndent + 1;
            RenderLine(builder, indent, "Exception sourceException = null;");
            RenderDeclareStatements(builder);
            RenderInitializeStatements(builder);

            Action<StringBuilder, int> renderFinally = _measurementRenderers.Count > 0
                ? (b, i) =>
                {
                    foreach (var measurementRenderer in _measurementRenderers)
                    {
                        measurementRenderer.RenderFinalize(b);
                    }
                }
                : null;

            RenderTry(
                builder,
                indent,
                (b, i) =>
                {
                    RenderFormatLine(
                        b,
                        i, 
                        "{0}{1}_decorated.{2}({3});",
                        returnsValue ? "return " : "",
                        isAsync ? "await ": "",
                        _methodName,
                        GetCallParameterList());
                },
                (b, i, ex) =>
                {
                    RenderFormatLine(b, i, "sourceException = {0};", ex);
                },
                renderFinally);
            RenderLine(builder, indent, "if(sourceException != null)");
            RenderLine(builder, indent, '{');
            RenderLine(builder, indent + 1, "ExceptionDispatchInfo.Throw(sourceException);");
            RenderLine(builder, indent, '}');
            if (returnsValue)
            {
                RenderLine(builder, indent, "return default; // Making the compiler happy. Will never be reached since 'ExceptionDispatchInfo.Throw' will always throw.");
            }
        }

        private string GetCallParameterList()
        {
            return string.Join(", ", _methodDeclaration.ParameterList.Parameters.Select(p => p.Identifier.Text));
        }
        private void RenderInitializeStatements(StringBuilder builder)
        {
            // Render initialize statements in reverse order so that attribute applied first will be executed closest to the called method
            for (var i = _measurementRenderers.Count - 1; i >= 0; --i)
            {
                _measurementRenderers[i].RenderInitialize(builder);
            }
        }

        private void RenderDeclareStatements(StringBuilder builder)
        {
            foreach (var measurementRenderer in _measurementRenderers)
            {
                measurementRenderer.RenderDeclare(builder);
            }
        }

        private void RenderSignature(StringBuilder builder, bool isAsync)
        {
            string returnType = _methodDeclaration.ReturnType.ToString();

            RenderFormatLine(builder, _baseIndent, "public {0}{1} {2}{3}", isAsync ? "async " : "", returnType,
                _methodName, _methodDeclaration.ParameterList.ToString());
        }

        private bool ReturnsValue()
        {
            return _methodDeclaration.ReturnType switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text != nameof(Task),
                PredefinedTypeSyntax preDefined => preDefined.Keyword.Text != "void",
                _ => true
            };
        }

        private bool IsMethodAsync()
        {
            return _methodDeclaration.ReturnType switch
            {
                IdentifierNameSyntax identifier => identifier.Identifier.Text == nameof(Task),
                GenericNameSyntax genericName => genericName.Identifier.Text == nameof(Task),
                _ => false
            };
        }
    }
}