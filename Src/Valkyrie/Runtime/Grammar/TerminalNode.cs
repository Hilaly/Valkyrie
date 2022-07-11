using System.Collections.Generic;

namespace Valkyrie.Grammar
{
    class TerminalNode : IAstNode
    {
        public readonly Lexem Lexem;

        public string Name => Lexem.Name;
        public IEnumerator<IAstNode> EnumerateTerminalNodes()
        {
            yield return this;
        }

        public List<IAstNode> GetChildren() => new List<IAstNode>();

        public TerminalNode(Lexem lexem)
        {
            Lexem = lexem;
        }

        public override string ToString()
        {
            return $"{Lexem}";
        }
    }
}