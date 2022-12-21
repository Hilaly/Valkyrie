using System.Collections.Generic;

namespace Valkyrie.DSL.Dictionary
{
    class DslDictionaryNode
    {
        public string Name { get; }

        private readonly List<DslDictionaryEntry> _entries = new();

        public IEnumerable<IDslDictionaryEntry> GetEntries => _entries;

        public DslDictionaryNode(string name) => Name = name;

        public void Add(DslDictionaryEntry dslDictionaryEntry) => _entries.Add(dslDictionaryEntry);
    }
}