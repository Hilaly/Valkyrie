using Valkyrie.Language.Description.Utils;

namespace Valkyrie.DSL.Definitions
{
    public interface IWritable
    {
        void Write(FormatWriter sb);
    }
}