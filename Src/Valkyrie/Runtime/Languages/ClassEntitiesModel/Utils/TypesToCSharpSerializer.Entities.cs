using System.Linq;
using Configs;
using UnityEngine;
using Utils;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie
{
    static partial class TypesToCSharpSerializer
    {
        static void WriteViewProperties(this BaseType baseType, FormatWriter sb)
        {
            foreach (var property in baseType.GetAllProperties(true))
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {property.GetMemberType()} {property.Name} => Model.{property.Name};");
            foreach (var info in baseType.GetAllInfos(true))
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {info.GetMemberType()} {info.Name} => Model.{info.Name};");
            foreach (var timer in baseType.GetAllTimers()) WriteViewModelTimer(timer, sb);

        }
        
        public static void WriteView(this BaseType baseType, FormatWriter sb)
        {
            if (!baseType.HasView)
                return;

            sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public partial class {baseType.Name}View : {typeof(MonoBehaviour).FullName}");
            sb.AppendLine($"public {baseType.Name} Model {{ get; internal set; }}");
            
            baseType.WriteViewProperties(sb);
            
            sb.EndBlock();
        }
        
        public static void WriteViewModels(this BaseType baseType, FormatWriter sb)
        {
            if (!baseType.HasView)
                return;

            sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public partial class {baseType.Name}ViewModel");
            sb.AppendLine($"public {baseType.Name} Model {{ get; }}");
            sb.BeginBlock($"public {baseType.Name}ViewModel({baseType.Name} model)");
            sb.AppendLine("Model = model;");
            sb.EndBlock();
            
            baseType.WriteViewProperties(sb);
            
            sb.EndBlock();
        }

    }
}