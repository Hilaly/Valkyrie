using System;
using System.Collections.Generic;
using UnityEngine.Scripting;
using Valkyrie.Utils;

namespace Valkyrie
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GraphAttribute : PreserveAttribute
    {
        public static readonly TypeCache<IGraph, GraphAttribute> Cache = new(
            (Type type, ref GraphAttribute storage, out Type key) =>
            {
                storage.Initialize(type);
                key = type;
                return true;
            });

        public HashSet<string> Tags { get; }

        public GraphAttribute(params string[] tags)
        {
            Tags = new HashSet<string>(tags);
        }

        private void Initialize(Type type)
        {
            //TODO: do some computations
        }
    }
}