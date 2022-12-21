using System;
using System.Reflection;

namespace Valkyrie.Model
{
    class PropertyInfoNodeProperty : INodeProperty
    {
        private readonly INode _instance;
        private readonly PropertyInfo _info;
        private readonly Action<INodeProperty> _callOnChanged;

        public PropertyInfoNodeProperty(INode instance, PropertyInfo info, string name,
            Action<INodeProperty> callOnChanged)
        {
            _instance = instance;
            _info = info;
            _callOnChanged = callOnChanged;
            Name = name;
        }

        public string Name { get; }

        public Type PropertyType => _info.PropertyType;

        public object Value
        {
            get => _info.GetValue(_instance);
            set
            {
                _info.SetValue(_instance, value);
                _callOnChanged?.Invoke(this);
            }
        }
    }
}