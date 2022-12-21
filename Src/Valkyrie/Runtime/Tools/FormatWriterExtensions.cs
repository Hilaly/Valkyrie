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

        public static FormatWriter Profile(this FormatWriter sb, string sectionName, Action internalWrite)
        {
            sb.AppendLine($"{typeof(UnityEngine.Profiling.Profiler).FullName}.BeginSample(\"{sectionName}\");");
            internalWrite();
            sb.AppendLine($"{typeof(UnityEngine.Profiling.Profiler).FullName}.EndSample(); // {sectionName}");
            return sb;
        }

        public static FormatWriter WriteRegion(this FormatWriter sb, string regionName, Action internalWrite)
        {
            sb.AppendLine($"#region {regionName}");
            sb.AppendLine();
            internalWrite();
            sb.AppendLine();
            sb.AppendLine($"#endregion //{regionName}");
            return sb;
        }
    }
}