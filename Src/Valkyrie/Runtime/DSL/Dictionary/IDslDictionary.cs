using System.Collections.Generic;

namespace Valkyrie.Ecs.DSL
{
    public interface IDslDictionary
    {
        IEnumerable<IDslDictionaryEntry> GetEntries { get; }
        void Load(string text);
    }
}