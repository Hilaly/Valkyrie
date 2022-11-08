using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Model;
using Valkyrie.Model.Nodes;

namespace Valkyrie.Utils
{
    internal static class CemCodeGenerator
    {
        public static void Generate(IGraph graph)
        {
            var world = new WorldModelInfo()
            {
                name = "Project",
                Namespace = "Naive"
            };

            var configs = graph.Nodes.OfType<ConfigNode>().ToList();
            var archetypes = graph.Nodes.OfType<ArchetypeNode>().ToList();

            foreach (var node in configs)
                BuildConfigType(node, configs, world, graph);
            foreach (var node in archetypes)
                BuildArchetypeType(node, archetypes, world, graph);

            world.Save("Assets", "GraphTemp.json");
            Debug.LogWarning(world);
        }

        static TType GetOrCreate<TType, TNode>(TNode node, WorldModelInfo world,
            IGraph graph, Action<TType> initCall)
            where TNode : class, INode 
            where TType : BaseType, new()
        {
            var type = world.Get<TType>(node.Name);
            if (type != null)
                return type;

            type = world.GetOrCreate<TType>(node.Name);

            foreach (var baseNode in graph.GetPreviousNodes(node.GetPort("Parents")))
            {
                var baseTypeNode = baseNode as TNode;
                var baseType = GetOrCreate<TType, TNode>(baseTypeNode, world, graph, initCall);
                type.Inherit(baseType);
            }
            
            initCall(type);
            
            return type;
        }

        private static EntityType BuildArchetypeType(ArchetypeNode node, List<ArchetypeNode> all, WorldModelInfo world,
            IGraph graph) =>
            GetOrCreate<EntityType, ArchetypeNode>(node, world, graph, type =>
            {

            });

        private static ConfigType BuildConfigType(ConfigNode node, List<ConfigNode> all, WorldModelInfo world,
            IGraph graph) =>
            GetOrCreate<ConfigType, ConfigNode>(node, world, graph, type =>
            {
                foreach (var previousNode in graph.GetPreviousNodes(node.GetPort("Properties")))
                {
                    var typedNode = previousNode as IPropertyNode;
                    var data = typedNode.Output;
                    type.AddProperty(data.Type, data.Name, true);
                }
                //TODO: init type!!!
            });
    }
}