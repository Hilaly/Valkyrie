using Valkyrie.Language.Description.Utils;

namespace Valkyrie
{
    public class WindowHandler : MethodImpl
    {
        public string Name { get; set; }

        public void Write(FormatWriter sb)
        {
            foreach (var op in _ops)
                op.Write(sb, OpType.Window);
        }
    }
}