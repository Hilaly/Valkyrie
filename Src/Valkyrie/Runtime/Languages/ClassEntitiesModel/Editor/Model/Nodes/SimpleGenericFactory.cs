using UnityEngine;

namespace Valkyrie.Model
{
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