using System.IO;

namespace Valkyrie.Grammar
{
    public interface IAstConstructor
    {
        ILexer GetLexer();
        IAstNode Parse(Stream scriptTextStream);
    }
}