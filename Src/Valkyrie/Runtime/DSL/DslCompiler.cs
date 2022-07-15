namespace Valkyrie.Ecs.DSL
{
    public class DslCompiler
    {
        private readonly DslDictionary _dslDictionary;

        public IDslDictionary Dictionary => _dslDictionary;

        public DslCompiler()
        {
            _dslDictionary = new DslDictionary();
        }
    }
}