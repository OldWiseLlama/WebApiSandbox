using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Generators
{
    public static class Helpers
    {
        public static void RenderIndent(StringBuilder builder, int indent)
        {
            builder.Append(' ', 4 * indent);
        }

        public static void RenderFormatLine(StringBuilder builder, int indent, string format, params object[] parameters)
        {
            if (indent > 0)
            {
                RenderIndent(builder, indent);
            }
            builder.AppendFormat(format, parameters);
            builder.AppendLine();
        }
        
        public static void RenderFormat(StringBuilder builder, int indent, string format, params object[] parameters)
        {
            if (indent > 0)
            {
                RenderIndent(builder, indent);
            }
            builder.AppendFormat(format, parameters);
        }
        
        public static void RenderLine(StringBuilder builder, int indent, string content)
        {
            RenderIndent(builder, indent);
            builder.Append(content);
            builder.AppendLine();
        }
        
        public static void RenderLine(StringBuilder builder, int indent, char content)
        {
            RenderIndent(builder, indent);
            builder.Append(content);
            builder.AppendLine();
        }

        public static void RenderTry(
            StringBuilder builder,
            int baseIndent,
            Action<StringBuilder, int> renderBody,
            Action<StringBuilder, int, string> renderCatch,
            Action<StringBuilder, int> renderFinally = null)
        {
            RenderLine(builder, baseIndent, "try");
            RenderLine(builder, baseIndent, "{");
            renderBody(builder, baseIndent + 1);
            RenderLine(builder, baseIndent, "}");
            RenderLine(builder, baseIndent, "catch(Exception e)");
            RenderLine(builder, baseIndent,"{");
            renderCatch(builder, baseIndent + 1, "e");
            RenderLine(builder, baseIndent,"}");
            if (renderFinally != null)
            {
                RenderLine(builder, baseIndent, "finally");
                RenderLine(builder, baseIndent,"{");
                renderFinally(builder, baseIndent + 1);
                RenderLine(builder, baseIndent,"}");
            }
        }
        
        public static (string context, string name) ResolveMeasurementContextAndName(
            AttributeSyntax measureAttribute,
            string classContext,
            string methodName)
        {
            if (measureAttribute.ArgumentList == null)
            {
                return (classContext, $"{classContext}{methodName}");
            }
            
            string context = null;
            string name = null;
            
            for (int i = 0; i < measureAttribute.ArgumentList.Arguments.Count; ++i)
            {
                var argument = measureAttribute.ArgumentList.Arguments[i];
                if (TryResolveFromNameColon(argument, ref context, ref name))
                {
                    continue;
                }

                ResolveFromIndex(argument, i, ref context, ref name);
            }

            context ??= classContext;
            
            return (context, name ?? $"{context}{methodName}");
        }

        private static void ResolveFromIndex(AttributeArgumentSyntax argument, int index, ref string context, ref string name)
        {
            switch (index)
            {
                case 0:
                    context = GetArgumentValueText(argument);
                    return;
                case 1:
                    if (name == null)
                    {
                        name = GetArgumentValueText(argument);
                    }
                    else
                    {
                        context = GetArgumentValueText(argument);
                    }

                    return;
            }
        }
        
        private static bool TryResolveFromNameColon(AttributeArgumentSyntax argument, ref string context, ref string name)
        {
            if (argument.NameColon is not null)
            {
                switch (argument.NameColon.Name.Identifier.Text)
                {
                    case "context":
                        context = GetArgumentValueText(argument);
                        return true;
                    case "name":
                        name = GetArgumentValueText(argument);
                        return true;
                }
            }

            return false;
        }
        
        private static string GetArgumentValueText(AttributeArgumentSyntax argument)
        {
            return argument.Expression.GetFirstToken().ValueText;
        }
    }
}