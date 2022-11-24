using System.Collections.Generic;
using System.Linq;
using Utils;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        static void WriteGeneralClassesAndInterfaces(IWorldInfo worldInfo, FormatWriter sb)
        {
            var archetypes = worldInfo.GetArchetypes();

            WriteGeneralInterfaces(sb, archetypes);
            WriteGeneralClasses(sb, archetypes);
        }

        private static void WriteGeneralClasses(FormatWriter sb, IReadOnlyList<IArchetypeInfo> archetypes)
        {
            sb.WriteRegion("General interfaces implementation", () =>
            {
                WriteWorldState(sb, archetypes);
                WriteWorldController(sb, archetypes);
            });
        }

        private static void WriteWorldState(FormatWriter sb, IReadOnlyList<IArchetypeInfo> archetypes)
        {
            sb.WriteBlock("class WorldState : IWorldState", () =>
            {
                sb.AppendLine($"private readonly {typeof(IEcsWorld).FullName} _ecsWorld;");
                sb.AppendLine();

                foreach (var archetype in archetypes)
                {
                    var structName = archetype.Name.Clean();
                    sb.AppendLine(
                        $"private readonly {typeof(GroupConverter<,>).Namespace}.GroupConverter<{structName}, {archetype.Name}> _{structName}Converter;");
                }

                sb.WriteBlock($"public WorldState({typeof(IEcsWorld).FullName} ecsWorld)", () =>
                {
                    sb.AppendLine("_ecsWorld = ecsWorld;");
                    sb.AppendLine();
                    foreach (var archetype in archetypes)
                    {
                        var structName = archetype.Name.Clean();
                        WriteGroupConstructor(structName, archetype, sb);
                    }
                });
                sb.AppendLine();
                foreach (var archetype in archetypes)
                {
                    var structName = archetype.Name.Clean();
                    sb.AppendLine(
                        $"IReadOnlyList<{archetype.Name}> IStateFilter<{archetype.Name}>.GetAll() => _{structName}Converter.AsConverted();");
                    if (archetype is NativeTypeEventArchetype)
                        sb.AppendLine(
                            $"public void Clear{structName}() => _{structName}Converter.AsEntities().ForEach(x => x.Destroy());");
                }
            });
        }

        private static void WriteWorldController(FormatWriter sb, IReadOnlyList<IArchetypeInfo> archetypes)
        {
            sb.WriteBlock("class WorldController : IWorldController", () =>
            {
                sb.AppendLine($"private readonly {typeof(IEcsWorld).FullName} _ecsWorld;");
                sb.AppendLine();

                foreach (var archetype in archetypes)
                {
                    var properties = CollectRequiredProperties(archetype);
                    var args = string.Join(", ",
                        properties.Select(x => $"{x.Key.GetTypeName()} {x.Key.Name.ConvertToUnityPropertyName()}"));
                    sb.AppendLine(
                        $"//Properties: {string.Join(",", properties.Select(x => $"{x.Key.Name}->{x.Key.GetTypeName()}"))}");
                    sb.WriteBlock($"public {archetype.Name} Create{archetype.Name.Clean()}({args})", () =>
                    {
                        sb.AppendLine(
                            $"var resultInstance = new {archetype.Name.Clean()} {{ Entity = _ecsWorld.State.CreateEntity() }};");
                        var components = properties.Select(x => x.Value).ToHashSet();
                        foreach (var component in components)
                        {
                            var template = GetComponentTemplate(component);
                            sb.AppendLine($"resultInstance.Entity.Add(new {component.Name.GetComponentFullName()} {{");
                            sb.AddTab();
                            foreach (var (property, _) in properties.Where(x => x.Value == component))
                                template.WriteInit(property, sb);
                            sb.RemoveTab();
                            sb.AppendLine($"}});");
                        }

                        sb.AppendLine("return resultInstance;");
                    });
                }
            });
        }

        private static void WriteGeneralInterfaces(FormatWriter sb, IReadOnlyList<IArchetypeInfo> archetypes)
        {
            sb.WriteRegion("General interfaces", () =>
            {
                var additional = "";
                if (archetypes.Any())
                    additional = " : \n\t\t" + string.Join(", \n\t\t",
                        archetypes.Select(x => $"{typeof(IStateFilter<>).Namespace}.IStateFilter<{x.Name}>"));

                sb.WriteBlock($"public interface IWorldState{additional}", () =>
                {
                    foreach (var archetype in archetypes)
                    {
                        if (archetype is NativeTypeEventArchetype)
                        {
                            sb.AppendLine($"//TODO: void Clear{archetype.Name.Clean()}();");
                        }
                    }
                });

                sb.AppendLine();

                sb.WriteBlock("public interface IWorldController", () =>
                {
                    foreach (var archetype in archetypes)
                    {
                        var properties = CollectRequiredProperties(archetype);
                        var args = string.Join(", ",
                            properties.Select(x => $"{x.Key.GetTypeName()} {x.Key.Name.ConvertToUnityPropertyName()}"));
                        sb.AppendLine(
                            $"//Properties: {string.Join(",", properties.Select(x => $"{x.Key.Name}->{x.Key.GetTypeName()}"))}");
                        sb.AppendLine($"{archetype.Name} Create{archetype.Name.Clean()}({args});");
                    }
                });
            });
            sb.AppendLine();
        }

        static IReadOnlyList<KeyValuePair<IPropertyInfo, IComponentInfo>> CollectRequiredProperties(
            IArchetypeInfo archetype)
        {
            var selectedComponents = archetype.Required.Select(x => archetype.Components.First(c => c.Name == x));
            var requiredProperties = new List<KeyValuePair<IPropertyInfo, IComponentInfo>>();
            foreach (var componentInfo in selectedComponents)
            {
                var template = GetComponentTemplate(componentInfo);
                var properties = template.GetRequiredProperties(componentInfo);
                foreach (var property in properties)
                    requiredProperties.Add(new KeyValuePair<IPropertyInfo, IComponentInfo>(property, componentInfo));
            }

            return requiredProperties;
        }

        static void WriteGroupConstructor(string name, IArchetypeFilter filter, FormatWriter sb)
        {
            sb.AppendLine($"_{name}Converter = new (");
            sb.AddTab();
            sb.AppendLine($"_ecsWorld.Groups.Build()");
            if (filter != null)
            {
                if (filter.Required.Any())
                    sb.AppendLine($".AllOf<{string.Join(", ", filter.Required.Select(GetComponentFullName))}>()");
                if (filter.Excluded.Any())
                    sb.AppendLine($".NotOf<{string.Join(", ", filter.Excluded.Select(GetComponentFullName))}>()");
            }

            sb.AppendLine(".Build() );");
            sb.RemoveTab();
        }
    }
}