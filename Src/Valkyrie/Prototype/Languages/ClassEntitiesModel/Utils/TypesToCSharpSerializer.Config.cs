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
                blockName += string.Join(", ", baseType.BaseTypes.Select(x => x.Name));
            else
                blockName += BaseConfigInterface;
            sb.BeginBlock(blockName);

            sb.AppendLine($"#region {BaseConfigInterface}");
            sb.AppendLine();
            sb.BeginBlock($"public override void PastLoad(IDictionary<string, {typeof(IConfigData).FullName}> configData)");
            sb.AppendLine("base.PastLoad(configData);");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine($"#endregion //{BaseConfigInterface}");
            sb.AppendLine();

            foreach (var property in baseType.Properties)
                property.WriteAsField(sb);

            sb.EndBlock();
        }

        private static void WriteConfigService(this WorldModelInfo world, FormatWriter sb)
        {
            const string typeName = "ProjectConfigService";
            sb.AppendLine("[CreateAssetMenu(menuName = \"Project/Config\")]");
            sb.BeginBlock($"public class {typeName} : {typeof(ScriptableConfigService).FullName}");
            sb.EndBlock();
            sb.AppendLine();

            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine();
            sb.AppendLine($"[UnityEditor.CustomEditor(typeof({typeName}), true)]");
            sb.BeginBlock("public class ConfigServiceEditor : UnityEditor.Editor");
            sb.AppendLine($"private {typeName} Model => ({typeName})target;");
            sb.BeginBlock("public override void OnInspectorGUI()");
            sb.AppendLine("base.OnInspectorGUI();");
            foreach (var type in world.Get<ConfigType>())
                sb.AppendLine($"if (GUILayout.Button(\"Create {type.Name}\")) Model.Create<{type.Name}>();");
            sb.AppendLine("GUILayout.Space(10);");
            sb.AppendLine("if(GUILayout.Button(\"Refresh\")) Model.Refresh();");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("#endif");
        }
    }
}