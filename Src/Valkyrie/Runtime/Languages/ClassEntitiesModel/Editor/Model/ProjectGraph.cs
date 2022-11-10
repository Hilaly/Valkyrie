using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Utils;
using Valkyrie.Model.Nodes;
using Valkyrie.Window;

namespace Valkyrie.Model
{
    class ProjectGraph : FeatureNode
    {
        public ProjectGraph()
        {
            Name = "Project";
        }

        public override IEnumerable<INodeFactory> GetFactories()
        {
            return base.GetFactories()
                    .Union(NodeFactories.GetProjectLevelNodes())
                ;

            return typeof(INodeFactory)
                .GetAllSubTypes(x =>
                    x.IsClass && !x.IsAbstract && x.GetConstructor(Type.EmptyTypes) != null &&
                    !x.ContainsGenericParameters)
                .Select(x => (INodeFactory)Activator.CreateInstance(x))
                .Union(new INodeFactory[]
                {
                    new TypeReferenceNode<ArchetypeNode>.Factory(),
                    new TypeReferenceNode<ConfigNode>.Factory(),
                });
        }

        public override void MarkDirty()
        {
            File.WriteAllText(CemWindow.fileName, JsonConvert.SerializeObject(this, CemWindow.SerializeSettings));
        }
    }
}