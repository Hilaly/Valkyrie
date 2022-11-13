using System;
using System.Collections.Generic;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Tools
{
    public static class FormatWriterExtensions
    {
        public static FormatWriter WriteUsing(this FormatWriter sb, IEnumerable<string> namespaces)
        {
            foreach (var ns in namespaces) 
                sb.AppendLine($"using {ns};");
            sb.AppendLine();
            return sb;
        }

        public static FormatWriter WriteInNamespace(this FormatWriter sb, string namespaceName, Action internalWrite)
        {
            sb.BeginBlock($"namespace {namespaceName}");
            internalWrite.Invoke();
            sb.EndBlock();
            return sb;
        }

        public static FormatWriter WriteBlock(this FormatWriter sb, string blockName, Action internalWrite)
        {
            sb.BeginBlock(blockName);
            internalWrite.Invoke();
            sb.EndBlock();
            return sb;
        }
    }
}