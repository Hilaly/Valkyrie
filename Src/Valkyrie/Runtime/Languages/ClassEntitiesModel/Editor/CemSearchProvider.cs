using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public class CemSearchProvider : ScriptableObject, ISearchWindowProvider
    {
        private GraphAttribute _graphTypeData;
        private bool _useGraphTagMatching;

        public BaseGraphView GraphView { get; private set; }

        public void Initialize(BaseGraphView baseGraphView)
        {
            GraphView = baseGraphView;
            bool found = GraphAttribute.Cache.TryGet(GraphView.Graph.GetType(), out _graphTypeData);
            _useGraphTagMatching = found && _graphTypeData?.Tags.Count > 0;
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
                Debug.Log($"[CEM] Add search section {group.Section.name}");
                foreach (var entry in group.Entries)
                {
                    tree.Add(entry);
                    Debug.Log($"[CEM] Add search entry {entry.name}");
                }
            }

            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            GraphView.CreateNode((IReflectionData)entry.userData, context.screenMousePosition);
            return true;
        }

        private IEnumerable<SearchGroup> GetSearchGroups()
        {
            Dictionary<string, SearchGroup> groups = new Dictionary<string, SearchGroup>();
            foreach (var node in NodeAttribute.Cache.All)
            {
                if (_useGraphTagMatching && !_graphTypeData.Tags.Overlaps(node.Tags)) continue;
                SearchGroup searchGroup = null;
                int depth = 1;

                foreach (string subsection in node.Path.Split('/'))
                {
                    var key = $"{subsection}{depth}";
                    if (!groups.TryGetValue(key, out searchGroup))
                    {
                        searchGroup = new SearchGroup(subsection, depth);
                        groups.Add(key, searchGroup);
                    }

                    depth++;
                }

                searchGroup?.Add(node);
            }

            var data = new List<SearchGroup>(groups.Values);
            data.Sort((a, b) => string.Compare(a.Section.name, b.Section.name, StringComparison.Ordinal));
            foreach (var group in data)
            {
                yield return group;
            }
        }
    }
}