using System.Linq;
using Configs;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie
{
    static partial class TypesToCSharpSerializer
    {
        private static string BaseConfigInterface => typeof(ScriptableConfigData).FullName;
        
        private static void WriteConfigClass(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public partial class {baseType.Name} : ";
            if (baseType.BaseTypes.Count > 0)
                blockName += string.Join(", ", baseType.BaseTypes.Select(x => x.Name)) + ", ";
            blockName += BaseConfigInterface;
            sb.BeginBlock(blockName);

            sb.AppendLine($"#region {BaseConfigInterface}");
            sb.AppendLine();
            sb.BeginBlock($"public override void PastLoad(IDictionary<string, {BaseConfigInterface}> configData)");
            sb.AppendLine("base.PastLoad(configData);");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine($"#endregion //{BaseConfigInterface}");
            sb.AppendLine();

            foreach (var property in baseType.Properties)
                property.WriteAsField(sb);

            sb.EndBlock();
        }
    }
}