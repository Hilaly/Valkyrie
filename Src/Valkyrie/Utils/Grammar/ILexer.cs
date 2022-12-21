using System.Collections.Generic;
using System.IO;

namespace Valkyrie.Grammar
{
    public interface ILexer
    {
        List<Lexem> Parse(Stream stream);
    }
}