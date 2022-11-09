using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Utils;
using Valkyrie.Window;

namespace Valkyrie.Model
{
    class OverAllGraph : CemGraph
    {
        public OverAllGraph()
        {
            Name = "All";
        }

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
}