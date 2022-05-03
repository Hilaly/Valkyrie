using System;

namespace Valkyrie.Grammar
{
    public class GrammarCompileException : Exception
    {
        public IAstNode Node { get; }

        public GrammarCompileException(IAstNode node, string msg = null) : base($"Failed to compile {node.Name} {msg ?? string.Empty}")
        {
            Node = node;
        }
    }
}