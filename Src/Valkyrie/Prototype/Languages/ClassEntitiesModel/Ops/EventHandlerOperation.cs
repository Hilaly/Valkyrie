using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Ops
{
    public abstract class EventHandlerOperation
    {
        public abstract bool IsAsync();
        public abstract void Write(FormatWriter sb, OpType opType);
    }
}