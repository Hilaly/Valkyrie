using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using Valkyrie.Window;

namespace Valkyrie.Model
{
    class OverAllGraph : CemGraph
    {
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return new[]
            {
                new ConfigNode.Factory()
            };
        }

        public override void MarkDirty()
        {
            File.WriteAllText(CemWindow.fileName, JsonConvert.SerializeObject(this, CemWindow.SerializeSettings));
        }
    }

    class SimpleGenericFactory<T> : INodeFactory where T : CemNode, new()
    {
        public string Name { get; }
        public string Path { get; }

        public SimpleGenericFactory(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public INode Create() => new T
        {
            Name = Name,
            NodeRect = new Rect(0, 0, 100, 50)
        };
    }

    class ConfigNode : CemNode
    {
        public class Factory : SimpleGenericFactory<ConfigNode>
        {
            public Factory() : base("Config", "Types")
            {
            }
        }
    }
}