using System;
using System.Collections.Generic;
using System.Linq;
using Configs;
using Meta.Inventory;
using UnityEngine;
using Utils;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie
{
    public static class ClassEntitiesExtensions
    {
        public static Type FindType(this string typeName)
        {
            switch (typeName)
            {
                case "bool":
                    return typeof(bool);
                case "float":
                    return typeof(float);
                case "string":
                    return typeof(string);
                case "int":
                    return typeof(int);
            }

            var allSubTypes = typeof(object).GetAllSubTypes(x => x.FullName == typeName || x.Name == typeName);
            var r = allSubTypes.FirstOrDefault(x => x.FullName == typeName)
                    ?? allSubTypes.FirstOrDefault();
            if (r == null)
                throw new Exception($"Couldn't find type {typeName}");
            return r;
        }

        public static TypeData ToTypeData(this BaseType type) => new RefTypeData(type);
        public static TypeData ToTypeData(this Type type) => new CSharpTypeData(type);

        public static TypeData ToTypeData(this string typeName)
        {
            var type = typeName.FindType();
            if (type == null)
                throw new Exception($"'{typeName}' is not valid type name");
            return type.ToTypeData();
        }
    }

    #region New Impl

    public abstract class TypeData
    {
        public abstract string GetTypeName();
    }

    class CSharpTypeData : TypeData
    {
        public readonly Type Type;

        public CSharpTypeData(Type type)
        {
            Type = type;
        }

        public override string GetTypeName()
        {
            if (Type.IsGenericType)
            {
                var result =
                    $"{Type.Namespace}.{Type.Name.Split('`')[0]}<{string.Join(',', Type.GetGenericArguments().Select(x => x.FullName))}>";
                return result;
            }

            return Type.FullName;
        }
    }

    class RefTypeData : TypeData
    {
        public readonly BaseType Type;

        public RefTypeData(BaseType type)
        {
            Type = type;
        }

        public override string GetTypeName() => Type.Name;
    }

    public interface INamed
    {
        public string Name { get; set; }
    }

    public interface IMember : INamed
    {
        string GetMemberType();
    }

    public interface IMemberGetter : INamed
    {
        public string GetMemberType();
    }

    public class TimerMember : IMemberGetter
    {
        public string Name { get; set; }

        public string GetMemberType()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class BaseTypeMember : INamed
    {
        public string Name { get; set; }
    }

    public abstract class TypedBaseTypeMember : BaseTypeMember, IMember, IMemberGetter
    {
        public TypeData TypeData;
        public virtual string GetMemberType() => TypeData.GetTypeName();
    }

    public class BaseTypeProperty : TypedBaseTypeMember
    {
        public bool IsRequired { get; set; }
    }

    public class BaseTypeInfo : TypedBaseTypeMember
    {
        public string Code;
    }

    #endregion

    #region Old

    public class InfoGetter : MemberInfo
    {
        public string Code;
    }

    public class MemberInfo : IMember
    {
        public string Name { get; set; }
        public string Type;

        string IMember.GetMemberType() => Type;
    }

    #endregion

    public interface IType : INamed
    {
        public HashSet<string> Attributes { get; }
        public BaseType AddProperty(string type, string name, bool isRequired);
    }

    public abstract class BaseType : IType
    {
        public string Name { get; set; }

        public IReadOnlyList<IMemberGetter> GetAllMemberGetters(bool includeGenerated)
        {
            var r = new List<IMemberGetter>();
            r.AddRange(GetAllProperties(includeGenerated));
            r.AddRange(GetAllInfos(includeGenerated));
            return r;
        }

        bool IsMemberExist(IMemberGetter newMember)
        {
            var existProperties = GetAllMemberGetters(false);
            foreach (var existMember in existProperties)
            {
                if (existMember.Name != newMember.Name)
                    continue;
                if (existMember.GetMemberType() != newMember.GetMemberType())
                    throw new Exception(
                        $"Type {Name} already has member {existMember.Name} with type {existMember.GetMemberType()}");
                return true;
            }

            return false;
        }

        #region Attributes

        public HashSet<string> Attributes { get; } = new();

        public BaseType AddAttribute(string attribute)
        {
            Attributes.Add(attribute);
            return this;
        }

        public IReadOnlyList<string> GetAllAttributes() => GetAllImplemented().SelectMany(x => x.Attributes).ToList();

        #endregion

        #region Inheritance

        public readonly List<BaseType> BaseTypes = new();

        public virtual BaseType Inherit(BaseType parent)
        {
            if (!BaseTypes.Contains(parent))
                BaseTypes.Add(parent);

            return this;
        }

        public IEnumerable<BaseType> GetAllImplemented() =>
            new HashSet<BaseType>(BaseTypes.SelectMany(entityBase => entityBase.GetAllImplemented())) { this };

        #endregion

        #region Properties

        public readonly List<BaseTypeProperty> Properties = new();

        public IReadOnlyList<BaseTypeProperty> GetAllProperties(bool includeGenerated)
        {
            var r = new List<BaseTypeProperty>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase =>
                entityBase.GetAllProperties(includeGenerated)))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Properties)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            if (includeGenerated)
            {
                foreach (var withPrefab in _syncWithPrefabs)
                    if (withPrefab.ViewName.NotNullOrEmpty())
                        r.Add(new BaseTypeProperty
                        {
                            Name = withPrefab.ViewName, TypeData = typeof(GameObject).ToTypeData(), IsRequired = false
                        });
            }

            return r;
        }

        public BaseType AddProperty(BaseType type, string name, bool isRequired) =>
            InternalAdd(new BaseTypeProperty
            {
                Name = name,
                TypeData = type.ToTypeData(),
                IsRequired = isRequired
            });

        public BaseType AddProperty(Type type, string name, bool isRequired) =>
            InternalAdd(new BaseTypeProperty
            {
                Name = name,
                TypeData = type.ToTypeData(),
                IsRequired = isRequired
            });

        public BaseType AddProperty<T>(string name, bool isRequired) =>
            InternalAdd(new BaseTypeProperty
            {
                Name = name,
                TypeData = typeof(T).ToTypeData(),
                IsRequired = isRequired
            });

        BaseType InternalAdd(BaseTypeProperty newProperty)
        {
            if (!IsMemberExist(newProperty))
                Properties.Add(newProperty);
            return this;
        }

        #endregion

        #region Attributes

        public virtual BaseType Singleton() => AddAttribute("singleton");
        public bool IsSingleton => GetAllAttributes().Contains("singleton");

        public BaseType View() => AddAttribute("view");
        public bool HasView => GetAllAttributes().Contains("view");

        #endregion

        #region Timers

        public readonly List<string> Timers = new();

        public BaseType AddTimer(string name)
        {
            if (!GetAllTimers().Contains(name))
                Timers.Add(name);
            return this;
        }

        public IReadOnlyCollection<string> GetAllTimers()
        {
            var s = new HashSet<string>(Timers);
            foreach (var baseType in BaseTypes)
                s.UnionWith(baseType.GetAllTimers());
            return s;
        }

        #endregion

        #region Infos

        public readonly List<BaseTypeInfo> Infos = new();

        public BaseType AddInfo(string type, string name, string code) =>
            InternalAdd(new BaseTypeInfo
            {
                Name = name,
                TypeData = type.ToTypeData(),
                Code = code
            });

        public BaseType AddInfo(Type type, string name, string code) =>
            InternalAdd(new BaseTypeInfo
            {
                Name = name,
                TypeData = type.ToTypeData(),
                Code = code
            });

        public BaseType AddInfo(BaseType type, string name, string code) =>
            InternalAdd(new BaseTypeInfo
            {
                Name = name,
                TypeData = type.ToTypeData(),
                Code = code
            });

        BaseType InternalAdd(BaseTypeInfo newMember)
        {
            if (!IsMemberExist(newMember))
                Infos.Add(newMember);
            return this;
        }

        public IReadOnlyList<BaseTypeInfo> GetAllInfos(bool addGenerated)
        {
            var r = new List<BaseTypeInfo>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllInfos(addGenerated)))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Infos)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            // TODO: add timers

            if (addGenerated)
            {
                foreach (var baseTypeMember in GetAllConfigs())
                {
                    foreach (var propertyInfo in ((RefTypeData)baseTypeMember.TypeData).Type.Properties)
                    {
                        var infoName = propertyInfo.Name;

                        if (r.Find(x => x.Name == infoName) != null)
                            continue;

                        r.Add(new BaseTypeInfo
                        {
                            TypeData = propertyInfo.TypeData,
                            Name = infoName,
                            Code = $"{baseTypeMember.Name}.{infoName}"
                        });
                    }
                }
            }

            return r;
        }

        #endregion

        public BaseType AddSlot(BaseType type, string name) => AddProperty(type, name, false);
        public BaseType AddConfig(BaseType type, string name) => AddProperty(type, name, true);

        public IReadOnlyList<TypedBaseTypeMember> GetAllConfigs() =>
            GetAllProperties(true)
                .Where(property => property.TypeData is RefTypeData { Type: ConfigType })
                .ToList();

        public IReadOnlyList<TypedBaseTypeMember> GetAllSlots() =>
            GetAllProperties(true)
                .Where(property => property.TypeData is RefTypeData { Type: EntityType })
                .ToList();

        #region Views prefabs

        private readonly List<ViewSpawnInfo> _syncWithPrefabs = new();

        public IReadOnlyList<ViewSpawnInfo> GetPrefabsProperties() => _syncWithPrefabs;

        public BaseType ViewWithPrefabByProperty(string propertyName, string viewReceiveProperty = null)
        {
            _syncWithPrefabs.Add(new ViewSpawnInfo { PropertyName = propertyName, ViewName = viewReceiveProperty });
            return View();
        }

        #endregion

        public BaseType AddProperty(string type, string name, bool isRequired)
        {
            return AddProperty(type.FindType(), name, isRequired);
        }
    }

    public class EntityType : BaseType
    {
    }

    public class ConfigType : BaseType
    {
    }

    public class ItemType : BaseType
    {
    }

    static class TypesToCSharpSerializer
    {
        private static string BaseConfigInterface => typeof(IConfigData).FullName;
        private static string BaseProfileInterface => typeof(BaseInventoryItem).FullName;

        public static void WriteViewModelTimer(string timer, FormatWriter sb)
        {
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public bool HasTimer{timer} => Model.{timer} != null;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}TimeLeft => Model.{timer}?.TimeLeft ?? 0f;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}Time => {timer}FullTime - {timer}TimeLeft;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}FullTime => Model.{timer}?.FullTime ?? 1f;");
            sb.AppendLine(
                $"[{typeof(BindingAttribute).FullName}] public float {timer}Progress => Mathf.Clamp01({timer}Time / {timer}FullTime);");
        }

        public static void WriteInterfaceTimer(string timer, FormatWriter sb)
        {
            sb.AppendLine($"ITimer {timer} {{ get; }}");
            sb.AppendLine($"void Start{timer}(float time);");
            sb.AppendLine($"void Stop{timer}();");
            sb.AppendLine($"bool {timer}JustFinished {{ get; }}");
        }

        public static void Write(this BaseTypeProperty property, FormatWriter sb) =>
            sb.AppendLine($"public {property.GetMemberType()} {property.Name} {{ get; set; }}");

        public static void WriteViewModels(this BaseType baseType, FormatWriter sb)
        {
            if (!baseType.HasView)
                return;

            sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public partial class {baseType.Name}ViewModel");
            sb.AppendLine($"public {baseType.Name} Model {{ get; }}");
            sb.BeginBlock($"public {baseType.Name}ViewModel({baseType.Name} model)");
            sb.AppendLine("Model = model;");
            sb.EndBlock();
            foreach (var property in baseType.GetAllProperties(true))
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {property.GetMemberType()} {property.Name} => Model.{property.Name};");
            foreach (var info in baseType.GetAllInfos(true))
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {info.GetMemberType()} {info.Name} => Model.{info.Name};");
            foreach (var timer in baseType.GetAllTimers()) WriteViewModelTimer(timer, sb);

            sb.EndBlock();
        }

        public static void WriteTypeClass(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public partial class {baseType.Name} : IEntity";
            if (baseType.BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", baseType.BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in baseType.GetAllProperties(true))
                property.Write(sb);
            foreach (var property in baseType.GetAllInfos(true))
                sb.AppendLine($"public {property.GetMemberType()} {property.Name} => {property.Code};");

            var timers = baseType.GetAllTimers();
            foreach (var timer in timers)
            {
                sb.AppendLine($"private EntityTimer {timer.ConvertToCamelCaseFieldName()};");
                sb.AppendLine(
                    $"public ITimer {timer} => {timer.ConvertToCamelCaseFieldName()} is {{ TimeLeft: > 0 }} ? {timer.ConvertToCamelCaseFieldName()} : {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.BeginBlock($"public void Start{timer}(float time)");
                sb.AppendLine($"if ({timer} != null) throw new Exception(\"Timer {timer} already exist\");");
                sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = new EntityTimer(time);");
                sb.EndBlock();
                sb.AppendLine($"public void Stop{timer}() => {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.AppendLine($"public bool {timer}JustFinished {{ get; private set; }}");
            }

            if (timers.Any())
            {
                sb.BeginBlock("internal void AdvanceTimers(float dt)");
                foreach (var timer in timers)
                {
                    sb.AppendLine($"{timer}JustFinished = false;");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()} != null)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()}.Advance(dt);");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()}.TimeLeft <= 0)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = default;");
                    sb.AppendLine($"{timer}JustFinished = true;");
                    sb.EndBlock();
                    sb.EndBlock();
                }

                sb.EndBlock();
            }

            sb.EndBlock();

            WriteViewModels(baseType, sb);
        }

        public static void WriteTypeInterface(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public interface {baseType.Name} : IEntity";
            if (baseType.BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", baseType.BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in baseType.Properties)
                property.Write(sb);

            foreach (var timer in baseType.Timers)
                WriteInterfaceTimer(timer, sb);

            foreach (var info in baseType.Infos)
                sb.AppendLine($"public {info.GetMemberType()} {info.Name} {{ get; }}");

            sb.EndBlock();

            WriteViewModels(baseType, sb);
        }

        public static void WriteConfigClass(this BaseType baseType, FormatWriter sb)
        {
            var blockName = $"public partial class {baseType.Name} : ";
            if (baseType.BaseTypes.Count > 0)
                blockName += string.Join(", ", baseType.BaseTypes.Select(x => x.Name)) + ", ";
            blockName += BaseConfigInterface;
            sb.BeginBlock(blockName);

            sb.AppendLine($"#region {BaseConfigInterface}");
            sb.AppendLine();
            if (baseType.BaseTypes.Any())
            {
                sb.BeginBlock($"public override void PastLoad(IDictionary<string, {BaseConfigInterface}> configData)");
                sb.AppendLine("base.PastLoad(configData);");
                sb.EndBlock();
            }
            else
            {
                sb.AppendLine("public string Id;");
                sb.AppendLine($"public string GetId() => Id;");
                sb.BeginBlock($"public virtual void PastLoad(IDictionary<string, {BaseConfigInterface}> configData)");
                sb.EndBlock();
            }

            sb.AppendLine();
            sb.AppendLine($"#endregion //{BaseConfigInterface}");
            sb.AppendLine();

            foreach (var property in baseType.Properties)
                property.Write(sb);

            sb.EndBlock();
        }

        public static void WriteInventoryClass(this BaseType baseType, FormatWriter sb)
        {
            var propertyAttributes = baseType.Attributes.Contains("view")
                ? $"[{typeof(BindingAttribute).FullName}] "
                : string.Empty;
            var blockName = $"{propertyAttributes}public partial class {baseType.Name} : ";
            if (baseType.BaseTypes.Count > 0)
                blockName += baseType.BaseTypes.Select(x => x.Name).Join(", ");
            else
                blockName += BaseProfileInterface;
            sb.BeginBlock(blockName);

            foreach (var property in baseType.Properties)
                property.Write(sb);

            sb.EndBlock();
        }
    }
}