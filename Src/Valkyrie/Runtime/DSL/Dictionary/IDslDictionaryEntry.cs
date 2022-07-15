namespace Valkyrie.Ecs.DSL
{
    public interface IDslDictionaryEntry
    {
        bool TryMatch(string text, LocalContext localContext);
    }
}