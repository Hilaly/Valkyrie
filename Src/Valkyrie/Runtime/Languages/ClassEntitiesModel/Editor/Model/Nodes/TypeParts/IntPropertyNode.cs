using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ListPropertyNode : CemNode
    {
        public class Factory : SimpleGenericFactory<ListPropertyNode>
        {
            public Factory() : base("List", "Properties") { }
        }

        [Output("Output"), JsonIgnore, DependsOnProperty("Name")]
        [field: JsonProperty]
        public PropertyDefine Output { get; } = new();

        [Input("Input"), JsonIgnore]
        public PropertyDefine Input
        {
            set => Output.Type = $"{typeof(List<>).Namespace}.List<{value.Type}>";
        }

        [ExportProperty(Name = "Name")]
        public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }
    }
    
    [Preserve]
    abstract class GenericPropertyNode<T> : CemNode
    {
        [Output("Output"), JsonIgnore, DependsOnProperty("Name")]
        [field: JsonProperty]
        public PropertyDefine Output { get; } = new()
        {
            Type = typeof(T).FullName
        };

        [ExportProperty(Name = "Name")] public string PropertyName
        {
            get => Output.Name;
            set => Output.Name = value;
        }
    }
    
    [Preserve]
    class StringPropertyNode : GenericPropertyNode<string>
    {
        public class Factory : SimpleGenericFactory<StringPropertyNode>
        {
            public Factory() : base("string", "Properties") { }
        }
    }
    [Preserve]
    class BoolPropertyNode : GenericPropertyNode<bool>
    {
        public class Factory : SimpleGenericFactory<BoolPropertyNode>
        {
            public Factory() : base("bool", "Properties") { }
        }
    }
    [Preserve]
    class IntPropertyNode : GenericPropertyNode<int>
    {
        public class Factory : SimpleGenericFactory<IntPropertyNode>
        {
            public Factory() : base("int", "Properties") { }
        }
    }
    [Preserve]
    class FloatPropertyNode : GenericPropertyNode<float>
    {
        public class Factory : SimpleGenericFactory<FloatPropertyNode>
        {
            public Factory() : base("float", "Properties") { }
        }
    }

    class PropertyDefine
    {
        public string Name;
        public string Type;
    }
}