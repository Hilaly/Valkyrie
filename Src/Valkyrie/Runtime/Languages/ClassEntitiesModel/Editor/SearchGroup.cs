using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    class SearchGroup
    {
        public SearchTreeGroupEntry Section { get; }
        public List<SearchTreeEntry> Entries { get; }

        public SearchGroup(string name, int depth)
        {
            Section = new SearchTreeGroupEntry(new GUIContent(name), depth);
            Entries = new List<SearchTreeEntry>();
        }

        public SearchGroup Add(INodeFactory data)
        {
            Entries.Add(new SearchTreeEntry(new GUIContent(data.Name)) { userData = data, level = Section.level + 1 });
            return this;
        }
    }
}