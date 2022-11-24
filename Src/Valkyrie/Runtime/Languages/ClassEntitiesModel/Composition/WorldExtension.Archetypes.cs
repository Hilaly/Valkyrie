using System;
using System.Collections.Generic;
using System.Reflection;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        static void WriteArchetypes(this IWorldInfo info, FormatWriter sb)
        {
            sb.WriteRegion("Archetypes", () =>
            {
                foreach (var archetype in info.GetArchetypes())
                    archetype.WriteArchetypeImplementation(sb);
            });
        }

        static void WriteArchetypeImplementation(this IArchetypeInfo archetypeInfo, FormatWriter sb)
        {
            var structName = archetypeInfo.Name.Clean();
            var header = $"class {structName}";
            header += $" : {typeof(IEntityWrapper)}, {archetypeInfo.Name.ToFullName()}";
            sb.WriteBlock(header, () =>
            {
                sb.AppendLine($"public {typeof(EcsEntity).FullName} Entity {{ get; set; }}");
                sb.AppendLine();
                foreach (var property in archetypeInfo.Properties)
                {
                    sb.WriteBlock($"{property.GetTypeName()} {archetypeInfo.Name.ToFullName()}.{property.Name}", () =>
                    {
                        var componentTemplate = GetComponentTemplate(archetypeInfo, property);
                        if (property.IsReadEnabled) componentTemplate.WriteGetter(property, sb);
                        if (property.IsWriteEnabled) componentTemplate.WriteSetter(property, sb);
                    });
                }

                sb.AppendLine();
                sb.AppendLine($"public static implicit operator {structName}({typeof(EcsEntity).FullName} e) => new() {{ Entity = e }};");
            });
        }

        internal static IEnumerable<NativePropertyInfo> CollectArchetypeProperties(this Type type)
        {
            if (!type.IsInterface)
                throw new Exception("Only interfaces can be used as Archetype");

            if (!typeof(IEntity).IsAssignableFrom(type))
                throw new Exception("Archetype must be convertible to IEntity");

            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var propertyInfo in properties)
            {
                //TODO: additional checks
                yield return new NativePropertyInfo(propertyInfo);
            }
        }
    }
}