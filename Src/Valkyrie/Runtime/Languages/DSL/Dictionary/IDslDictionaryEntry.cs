using System.Collections.Generic;
using Valkyrie.Grammar;

namespace Valkyrie.DSL.Dictionary
{
    public interface IDslDictionaryEntry
    {
        bool TryMatch(string text, LocalContext localContext);

        bool TryMatch(List<IAstNode> sentence, LocalContext localContext);
    }
}