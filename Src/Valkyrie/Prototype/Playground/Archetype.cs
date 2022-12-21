using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Valkyrie.Playground
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ArchetypeComponentAttribute : Attribute
    {
        public Type RequiredType { get; }

        public ArchetypeComponentAttribute(Type requiredType)
        {
            RequiredType = requiredType;
        }
    }
    
    public abstract class Archetype : IArchetype
    {
        private readonly List<Type> _requiredComponents = new();
        
        protected Archetype()
        {
            var attributes = this.GetType().GetCustomAttributes<ArchetypeComponentAttribute>();
            foreach (var attribute in attributes)
                if (!_requiredComponents.Contains(attribute.RequiredType))
                    _requiredComponents.Add(attribute.RequiredType);
        }

        public void Prepare(IEntity e)
        {
            var exist = e.GetAll<IComponent>();
            foreach (var requiredComponent in _requiredComponents)
            {
                if(exist.Any(x => requiredComponent.IsInstanceOfType(x)))
                    continue;
                var c = e.Add(requiredComponent);
            }
        }
    }
}