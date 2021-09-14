using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Generators.Helpers;

namespace Generators
{
    public abstract class MeasurementRenderer
    {
        private readonly int _baseIndent;
        private readonly string _methodName;
        
        protected readonly int MeasurementIndex;
        protected readonly string MeasurementContext;
        protected readonly string MeasurementName;

        private static readonly Dictionary<string, Func<int, int, AttributeSyntax, string, string, MeasurementRenderer>> Activators;

        static MeasurementRenderer()
        {
            Activators = Assembly.GetExecutingAssembly()
                .GetTypes().Where(t => typeof(MeasurementRenderer).IsAssignableFrom(t) && !t.IsAbstract)
                .ToDictionary(t => t.Name.Replace("Renderer", ""), CreateActivator);
        }
        
        protected MeasurementRenderer(int measurementIndex, int baseIndent, AttributeSyntax attribute, string classContext, string methodName)
        {
            var (context, name) = ResolveMeasurementContextAndName(attribute, classContext, methodName);
            MeasurementIndex = measurementIndex;
            _baseIndent = baseIndent;
            _methodName = methodName;
            MeasurementContext = context;
            MeasurementName = name;
        }

        public abstract IEnumerable<string> GetNamespaces();

        public virtual IEnumerable<DependencyInfo> GetDependencies()
        {
            return Enumerable.Empty<DependencyInfo>();
        }

        public void RenderDeclare(StringBuilder builder)
        {
            if (HasInitialize)
            {
                RenderFormatLine(
                    builder,
                    _baseIndent, 
                    "var {0}_{1}_InitializeFailed = false;", 
                    MeasurementName,
                    MeasurementIndex);
            }

            RenderDeclareContent(builder, _baseIndent);
        }

        protected virtual void RenderDeclareContent(StringBuilder builder, int indent)
        {
            
        }

        public void RenderInitialize(StringBuilder builder)
        {
            if (!HasInitialize)
            {
                return;
            }
            
            RenderTry(
                builder,
                _baseIndent,
                RenderInitializeContent,
                (b, i, ex) =>
                {
                    RenderFormatLine(b, i,
                        "_logger.LogError({0}, \"Failed initializing {{name}} measurement for {{method}} in context {{context}}\", \"{1}\", \"{2}\", \"{3}\");",
                        ex, MeasurementName, _methodName, MeasurementContext);
                    RenderFormatLine(b, i, "{0}_{1}_InitializeFailed = true;",
                        MeasurementName,
                        MeasurementIndex);
                });
        }

        protected virtual void RenderInitializeContent(StringBuilder builder, int indent)
        {
            
        }

        public void RenderFinalize(StringBuilder builder)
        {
            if (!HasFinalize)
            {
                return;
            }

            Action<StringBuilder, int> renderBody = HasInitialize
                ? (b, i) =>
                {
                    RenderFormatLine(b, i, "if(!{0}_{1}_InitializeFailed)", MeasurementName,
                        MeasurementIndex);
                    RenderLine(b, i, '{');
                    RenderFinalizeContent(b, i+ 1);
                    RenderLine(b, i, '}');
                }
                : RenderFinalizeContent;

            RenderTry(
                builder,
                _baseIndent + 1,
                renderBody,
                (b, i, ex) =>
                {
                    RenderFormatLine(b, i,
                        "_logger.LogError({0}, \"Failed finalizing {{name}} measurement for {{method}} in context {{context}}\", \"{1}\", \"{2}\", \"{3}\");",
                        ex, MeasurementName, _methodName, MeasurementContext);
                }
            );
        }

        protected virtual void RenderFinalizeContent(StringBuilder builder, int indent)
        {
            
        }

        protected abstract bool HasInitialize { get; }
        
        protected abstract bool HasFinalize { get; }

        public static IEnumerable<MeasurementRenderer> Create(int baseIndent, string methodName, string classContext, MethodDeclarationSyntax methodDeclaration)
        {
            int index = 0;
            foreach (var attributeList in methodDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var currentAttribute = attribute;
                    if (!Activators.TryGetValue(currentAttribute.Name.GetText().ToString(), out var activator))
                    {
                        continue;
                    }

                    yield return activator(index++, baseIndent, attribute, classContext, methodName);
                }
            }
        }

        private static Func<int, int, AttributeSyntax, string, string, MeasurementRenderer> CreateActivator(Type rendererType)
        {
            return (measurementIndex, baseIndent, attribute, classContext, methodName) =>
                // The Activator.CreateInstance is not the most performant thing ever but it will do since this will be executed at compile time only so performance is not that big of an issue.
                (MeasurementRenderer) Activator.CreateInstance(rendererType, measurementIndex, baseIndent, attribute, classContext, methodName);
        }
    }
}