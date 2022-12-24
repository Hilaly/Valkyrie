using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Valkyrie.Defines;

namespace Valkyrie
{
    public class GameDescription
    {
        #region Types

        internal readonly Dictionary<string, ITypeDefine> Types = new();

        public TypeReference GetTypeReference(string name)
        {
            var exist = Types.Values.FirstOrDefault(x => x.Name == name || x.GetFullName() == name);
            if (exist != null)
                return new TypeReference() { DefinedType = exist };

            if (_definedComponents.TryGetValue(name, out var component))
                return new TypeReference() { BaseDescription = component };

            if (_definedArchetypes.TryGetValue(name, out var archetype))
                return new TypeReference() { BaseDescription = archetype };


            var e = typeof(object).GetAllSubTypes(x => x.Name == name || x.FullName == name);
            if (e.Any())
                return new TypeReference() { DefinedType = Import(e.First()) };

            throw new Exception($"Couldn't find type reference {name}");
        }

        public ITypeDefine Import(Type type)
        {
            var r = new NativeTypeDefine(type);
            Types.Add(r.GetFullName(), r);
            return r;
        }

        #endregion

        #region Components

        private readonly Dictionary<string, IComponentDescription> _definedComponents = new();

        public IEnumerable<IComponentDescription> GetComponents() => _definedComponents.Values;

        public IComponentDescription GetComponent(string componentName) =>
            _definedComponents.TryGetValue(componentName, out var exist)
                ? exist
                : default;

        public void TryAddComponent(string componentName, string componentType)
        {
            var type = GetTypeReference(componentType);
            if (_definedComponents.TryGetValue(componentName, out var exist))
            {
                if (!exist.Type.Equals(type))
                    throw new Exception($"Component {componentName} already defined with other type");
                return;
            }

            var realName = componentName.ConvertToCamelCasePropertyName();
            if (!realName.EndsWith("Component"))
                realName = $"{realName}Component";
            exist = new ComponentDescription()
            {
                Name = realName,
                IsLocked = false,
                Type = type
            };
            _definedComponents.Add(componentName, exist);
        }

        #endregion

        #region Archetypes

        readonly Dictionary<string, IArchetypeDescription> _definedArchetypes = new();

        public IEnumerable<IArchetypeDescription> GetArchetypes() => _definedArchetypes.Values;

        public IArchetypeDescription TryGetArchetype(string archetypeName, bool createIfNotExist)
        {
            var realName = $"{archetypeName.ConvertToCamelCasePropertyName()}Archetype";
            if (_definedArchetypes.TryGetValue(archetypeName, out var exist))
                return exist;

            if (createIfNotExist)
            {
                exist = new ArchetypeDescription()
                {
                    Name = realName,
                    IsLocked = false
                };
                _definedArchetypes.Add(archetypeName, exist);
            }

            return exist;
        }

        #endregion
    }

    public interface IBaseDescription
    {
        public string Name { get; }
        public bool IsLocked { get; }
    }

    public interface IComponentDescription : IBaseDescription
    {
        TypeReference Type { get; set; }
    }

    public interface IArchetypeDescription : IBaseDescription
    {
        public IReadOnlyList<IComponentDescription> Components { get; }
        void TryAddComponent(IComponentDescription component);
    }

    class BaseDescription : IBaseDescription
    {
        public string Name { get; set; }
        public bool IsLocked { get; set; }
    }

    class ComponentDescription : BaseDescription, IComponentDescription
    {
        public TypeReference Type { get; set; }
    }

    class ArchetypeDescription : BaseDescription, IArchetypeDescription
    {
        private readonly List<IComponentDescription> _components = new();

        public IReadOnlyList<IComponentDescription> Components => _components;

        public void TryAddComponent(IComponentDescription component)
        {
            if (IsLocked)
                throw new Exception($"Archetype {Name} is locked");
            
            if (component == null)
                throw new Exception($"Component can not be null");
            if(_components.Contains(component))
                return;
            var exist = _components.Find(x => x.Name == component.Name);
            if(exist != null)
                throw new Exception($"Component {component.Name} already exist");
            _components.Add(component);
        }
    }
}