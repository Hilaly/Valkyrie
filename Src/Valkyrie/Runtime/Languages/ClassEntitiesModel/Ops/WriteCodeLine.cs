using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Ops
{
    class WriteCodeLine : EventHandlerOperation
    {
        private string _code;

        public override bool IsAsync() => false;

        public WriteCodeLine(string code)
        {
            if (code.EndsWith(";"))
                _code = code;
            else
                _code = code + ";";
        }

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine(_code);
        }
    }
}