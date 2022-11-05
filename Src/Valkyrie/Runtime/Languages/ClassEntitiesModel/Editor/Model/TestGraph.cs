using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Utils;
using Valkyrie.Window;

namespace Valkyrie.Model
{
    class OverAllGraph : CemGraph
    {
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return typeof(INodeFactory)
                .GetAllSubTypes(x => x.IsClass && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null)
                .Select(x => (INodeFactory)Activator.CreateInstance(x));
        }

        public override void MarkDirty()
        {
            File.WriteAllText(CemWindow.fileName, JsonConvert.SerializeObject(this, CemWindow.SerializeSettings));
        }
    }

    abstract class SimpleGenericFactory<T> : INodeFactory where T : CemNode, new()
    {
        public string Name { get; }
        public string Path { get; }

        public SimpleGenericFactory(string name, string path)
        {
            Name = name;
            Path = path;
        }

        public INode Create()
        {
            var r = CreateNode();
            return r;
        }

        protected virtual T CreateNode() =>
            new T
            {
                Name = Name,
                NodeRect = new Rect(0, 0, 100, 50)
            };
    }
}