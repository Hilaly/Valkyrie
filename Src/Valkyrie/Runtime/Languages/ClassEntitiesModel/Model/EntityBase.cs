using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Valkyrie.Tools;

namespace Valkyrie
{
    public static class ClassEntitiesExtensions
    {
        private static readonly Regex ListMatch = new Regex(@"List<(?<value>[\d\w\.]+)>");
        
        public static Type FindType(this string typeName, bool throwOnError = true)
        {
            var match = ListMatch.Match(typeName);
            if (match.Success)
                return typeof(List<>).MakeGenericType(FindType(match.Groups["value"].Value));
            
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
            if (r == null && throwOnError)
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

        public static T Inherit<T, TBase>(this T baseType, WorldModelInfo world) 
            where T : BaseType
        {
            var registered = world.Get<T>(typeof(TBase).FullName);
            Debug.Assert(registered != null, $"{typeof(TBase)} not registered");
            baseType.Inherit(registered);
            return baseType;
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

    class ViewTypeData : TypeData
    {
        public readonly BaseType Type;

        public ViewTypeData(BaseType type)
        {
            Type = type;
        }

        public override string GetTypeName() => $"{Type.Name}View";
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

        public string GetFixedName() => Name.Replace(".", "");

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
                TryAddProperty(r, propertyInfo);

            foreach (var propertyInfo in Properties)
                TryAddProperty(r, propertyInfo);

            if (includeGenerated)
            {
                foreach (var withPrefab in _syncWithPrefabs)
                    if (withPrefab.ViewName.NotNullOrEmpty())
                        r.Add(new BaseTypeProperty
                        {
                            Name = withPrefab.ViewName, TypeData = new ViewTypeData(this), IsRequired = false
                        });
            }

            return r;
        }

        private static void TryAddProperty(List<BaseTypeProperty> r, BaseTypeProperty propertyInfo)
        {
            if (r.Contains(propertyInfo)) 
                return;
            
            var existed = r.Find(x => x.Name == propertyInfo.Name);
            if (existed != null)
            {
                if (existed.TypeData.GetTypeName() != propertyInfo.TypeData.GetTypeName())
                    throw new Exception($"2 properties with different types");
            }
            else
                r.Add(propertyInfo);
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
        [JsonIgnore] public bool IsNative => Attributes.Contains("native");
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
            if(_syncWithPrefabs.TrueForAll(x => x.PropertyName != propertyName && x.ViewName != viewReceiveProperty))
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
    
    public class WindowType : BaseType
    {
        public readonly List<InfoGetter> Bindings = new();
        public readonly List<WindowHandler> Handlers = new();

        public string ClassName => $"{Name}Window";
        
        public string GetButtonEvent(string buttonName)
        {
            return $"On{buttonName}ButtonAt{Name}Clicked";
        }

        public WindowHandler AddHandler(string name)
        {
            var r = new WindowHandler() { Name = name };
            Handlers.Add(r);
            return r;
        }

        public WindowHandler DefineButton(string buttonName, EventEntity evType)
        {
            var r = AddHandler($"On{buttonName}Clicked");
            r.RaiseOp(evType);
            return r;
        }
    }
}