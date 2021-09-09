using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators
{
    [Generator]
    public class MetricsDecoratorGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            // Uncomment if you need to debug the generator. Requires Visual Studio!
            // if (!Debugger.IsAttached)
            // {
            //     Debugger.Launch();
            // }
#endif
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var syntaxTrees = context.Compilation.SyntaxTrees;

            foreach (var syntaxTree in syntaxTrees.Where(tree => tree.HasCompilationUnitRoot))
            {
                if (syntaxTree.GetRoot() is not CompilationUnitSyntax root)
                {
                    continue;
                }

                HandleCompilationUnit(context, root);
            }
        }

        private static void HandleCompilationUnit(GeneratorExecutionContext context, CompilationUnitSyntax root)
        {
            var namespaceDeclaration = root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

            var typesToDecorate = GetTypesToDecorate(root);

            foreach (var type in typesToDecorate)
            {
                GenerateDecorator(context, namespaceDeclaration, root.Usings, type);
            }
        }

        private static void GenerateDecorator(
            GeneratorExecutionContext context,
            NamespaceDeclarationSyntax namespaceDeclaration,
            SyntaxList<UsingDirectiveSyntax> usingStatements,
            TypeDeclarationSyntax typeToDecorate)
        {
            var compilationUnitRenderer =
                new CompilationUnitRenderer(namespaceDeclaration.Name.GetText().ToString(), usingStatements, typeToDecorate);
            var builder = new StringBuilder();
            
            compilationUnitRenderer.Render(builder);

            context.AddSource(compilationUnitRenderer.GetGeneratedTypeName(), builder.ToString());
        }

        private static IEnumerable<TypeDeclarationSyntax> GetTypesToDecorate(CompilationUnitSyntax root)
        {
            return root
                .DescendantNodes()
                .OfType<TypeDeclarationSyntax>()
                .Where(t =>
                    t.AttributeLists
                        .Any(a => a.Attributes.Any(at => at.Name.GetText().ToString() == "GatherMetrics")));
        }
    }
}