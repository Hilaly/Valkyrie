using System.Collections.Generic;

namespace Valkyrie.DSL.Dictionary
{
    public interface IDslDictionary
    {
        IEnumerable<IDslDictionaryEntry> GetEntries { get; }
        void Load(string text);
    }
}