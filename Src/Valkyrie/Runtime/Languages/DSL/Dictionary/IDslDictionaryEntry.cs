namespace Valkyrie.DSL.Dictionary
{
    public interface IDslDictionaryEntry
    {
        bool TryMatch(string text, LocalContext localContext);
    }
}