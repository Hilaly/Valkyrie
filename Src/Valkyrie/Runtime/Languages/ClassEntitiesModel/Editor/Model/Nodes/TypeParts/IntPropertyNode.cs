using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ListPropertyNode : CemNode
    {
        private PropertyDefine _output;
        
        [Output("Output"), JsonIgnore] public PropertyDefine Output => _output;

        [Input("Input"), JsonIgnore]
        public PropertyDefine Input
        {
            set => _output.Type = $"{typeof(List<>).Namespace}.List<{value.Type}>";
        }
    }
    
    [Preserve]
    abstract class GenericPropertyNode<T> : CemNode
    {
        private readonly PropertyDefine _propertyDefine = new()
        {
            Type = typeof(T).FullName
        };
        
        protected GenericPropertyNode()
        {
            CreateProperty("PropertyName", "Name");
        }

        public override void OnCreate()
        {
            base.OnCreate();

            CreateOutputPort<PropertyDefine>("Output");
        }

        [JsonIgnore] private PropertyDefine Output => _propertyDefine;

        public string PropertyName
        {
            get => _propertyDefine.Name;
            set
            {
                _propertyDefine.Name = value;
                OnNodeChanged(CemNodeChangedEvent.PortValueChanged(GetPort("Output")));
            }
        }
    }
    
    [Preserve]
    class StringPropertyNode : GenericPropertyNode<string>
    {
        public class Factory : SimpleGenericFactory<StringPropertyNode>
        {
            public Factory() : base("string", "Properties")
            {
            }
        }
    }
    [Preserve]
    class BoolPropertyNode : GenericPropertyNode<bool>
    {
        public class Factory : SimpleGenericFactory<BoolPropertyNode>
        {
            public Factory() : base("bool", "Properties")
            {
            }
        }
    }
    [Preserve]
    class IntPropertyNode : GenericPropertyNode<int>
    {
        public class Factory : SimpleGenericFactory<IntPropertyNode>
        {
            public Factory() : base("int", "Properties")
            {
            }
        }
    }
    [Preserve]
    class FloatPropertyNode : GenericPropertyNode<float>
    {
        public class Factory : SimpleGenericFactory<FloatPropertyNode>
        {
            public Factory() : base("float", "Properties")
            {
            }
        }
    }

    class PropertyDefine
    {
        public string Name;
        public string Type;
    }
}