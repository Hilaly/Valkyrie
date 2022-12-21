namespace Valkyrie.DSL.Dictionary
{
    class ExtractTreeFormatEntry : DslDictionaryFormatEntry
    {
        public readonly DslDictionary Dictionary;

        public ExtractTreeFormatEntry(string text, DslDictionary dictionary) : base(text)
        {
            Dictionary = dictionary;
        }
    }
}