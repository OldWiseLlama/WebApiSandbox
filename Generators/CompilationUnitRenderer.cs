using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public class CompilationUnitRenderer
    {
        private readonly ClassRenderer _classRenderer;
        private readonly string _namespaceName;
        private readonly SyntaxList<UsingDirectiveSyntax> _usingDirectives;

        public CompilationUnitRenderer(string namespaceName, SyntaxList<UsingDirectiveSyntax> usingDirectives, TypeDeclarationSyntax typeToDecorate)
        {
            _classRenderer = new ClassRenderer(1, typeToDecorate);
            _namespaceName = namespaceName;
            _usingDirectives = usingDirectives;
        }

        public string GetGeneratedTypeName()
        {
            return _classRenderer.GetGeneratedTypeName();
        }
        
        public void Render(StringBuilder builder)
        {
            RenderUsingDirectives(builder);
            builder.AppendLine();
            RenderFormatLine(builder, 0, "namespace {0}", _namespaceName);
            RenderLine(builder, 0, '{');
            _classRenderer.Render(builder);
            RenderLine(builder, 0, '}');
        }

        private void RenderUsingDirectives(StringBuilder builder)
        {
            foreach (var namespaceName in _usingDirectives.Select(u => u.Name.GetText().ToString()).Union(_classRenderer.GetNamespaces()).Distinct())
            {
                RenderFormatLine(builder, 0, "using {0};", namespaceName);
            }
        }
    }
}