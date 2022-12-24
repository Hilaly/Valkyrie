using System.Collections.Generic;
using System.Linq;
using Valkyrie.Defines;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Playground;
using Valkyrie.Tools;

namespace Valkyrie
{
    public static class PlaygroundCompiler
    {
        const string namespaceName = "GeneratedPlayground";
        
        public static void CompileToPlayground(this GameDescription gameDescription, string pathDirectory)
        {
            var methods = new List<KeyValuePair<string, string>>();

            foreach (var data in gameDescription.Types.Values
                .Where(x => x is not NativeTypeDefine))
                PExtensions.WriteToSeparateFile(methods, string.Empty, $"{data.Name}.cs",
                    sb => sb.Write(data));

            foreach (var description in gameDescription.GetComponents().Where(x => !x.IsLocked))
                PExtensions.WriteToSeparateFile(methods, "Components", $"{description.Name}.cs",
                    sb => WritePlaygroundComponent(sb, description));

            foreach (var description in gameDescription.GetArchetypes().Where(x => !x.IsLocked))
                PExtensions.WriteToSeparateFile(methods, "Archetypes", $"{description.Name}.cs",
                    sb => WritePlaygroundArchetype(sb, description));

            PExtensions.WriteToDirectory(pathDirectory, methods);
        }

        private static void WritePlaygroundArchetype(FormatWriter sb, IArchetypeDescription archetypeDescription)
        {
            var name = archetypeDescription.Name;
            var components = archetypeDescription.Components;

            sb.WriteInNamespace(namespaceName, () =>
            {
                var str = $"public class {name}";
                sb.WriteBlock(str, () => { });
            });
        }

        private static void WritePlaygroundComponent(FormatWriter sb, IComponentDescription componentDescription)
        {
            var compName = componentDescription.Name;
            var compType = componentDescription.Type.GetTypeString();

            //1. Component interface
            sb.WriteInNamespace(namespaceName, () =>
            {
                var str = $"public interface I{compName} : {typeof(ITypedComponent<>).WriteFullTypeName()}<{compType}>";
                sb.WriteBlock(str, () => { });
            });
            sb.AppendLine();
            //2. Component realization
            sb.WriteInNamespace(namespaceName, () =>
            {
                var str = $"class {compName} : {typeof(TypedComponent<>).WriteFullTypeName()}<{compType}>, I{compName}";
                sb.WriteBlock(str, () => { });
            });
            sb.AppendLine();
        }
    }
}