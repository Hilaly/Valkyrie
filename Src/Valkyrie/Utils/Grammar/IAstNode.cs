using System.Collections.Generic;

namespace Valkyrie.Grammar
{
    public interface IAstNode
    {
        string Name { get; }

        IEnumerator<IAstNode> EnumerateTerminalNodes();
        List<IAstNode> GetChildren(bool unpackGenerated = true);
    }
}