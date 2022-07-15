namespace Valkyrie.Ecs.DSL
{
    abstract class DslDictionaryFormatEntry
    {
        public string Text { get; }

        protected DslDictionaryFormatEntry(string text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}