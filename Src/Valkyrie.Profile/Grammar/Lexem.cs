namespace Valkyrie.Grammar
{
    public class Lexem
    {
        public string Name;
        public string Value;

        public override string ToString()
        {
            return $"{Name}[{Value}]";
        }
    }
}