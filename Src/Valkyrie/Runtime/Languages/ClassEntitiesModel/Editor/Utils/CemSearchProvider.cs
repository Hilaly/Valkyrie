using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils;

namespace Valkyrie.View
{
    class CemSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private CemGraphView _graphView;
        
        public void Initialize(CemGraphView graphView)
        {
            _graphView = graphView;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
            };
            foreach (var group in GetSearchGroups())
            {
                tree.Add(group.Section);
                Debug.LogWarning($"[CEM] Add search section {group.Section.name} d={group.Section.level}");
                foreach (var entry in group.Entries)
                {
                    tree.Add(entry);
                    Debug.LogWarning($"[CEM] Add search entry {entry.name} d={entry.level}");
                }
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            _graphView.CreateNode((Model.INodeFactory)searchTreeEntry.userData, context.screenMousePosition);
            return true;
        }
        
        private IEnumerable<SearchGroup> GetSearchGroups()
        {
            Dictionary<string, SearchGroup> groups = new Dictionary<string, SearchGroup>();
            foreach (Model.INodeFactory factory in CollectFactories())
            {
                //TODO: if (!_graphTypeData.Tags.Overlaps(factory.Tags)) continue;
                
                SearchGroup searchGroup = null;
                int depth = 1;

                foreach (string subsection in factory.Path.Split('/'))
                {
                    var key = $"{subsection}{depth}";
                    if (!groups.TryGetValue(key, out searchGroup))
                    {
                        searchGroup = new SearchGroup(subsection, depth);
                        groups.Add(key, searchGroup);
                    }

                    depth++;
                }

                searchGroup?.Add(factory);
            }

            var data = new List<SearchGroup>(groups.Values);
            data.Sort((a, b) => string.Compare(a.Section.name, b.Section.name, StringComparison.Ordinal));
            foreach (var group in data)
            {
                yield return group;
            }
        }

        private IEnumerable<Model.INodeFactory> CollectFactories()
        {
            return _graphView.Graph.GetFactories();
            /*
            var nodes = typeof(INode).GetAllSubTypes(x => x.IsClass && !x.IsAbstract)
                .Where(x => x.GetConstructor(Type.EmptyTypes) != null)
                .Select(x => (INode)Activator.CreateInstance(x))
                .Select(x => x.GetData());
            var set = new HashSet<INodeFactory>();
            set.UnionWith(nodes);
            return set;
            */
        }
    }
}