using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Ops
{
    class LogOperation : EventHandlerOperation
    {
        private readonly string _text;

        public LogOperation(string text)
        {
            _text = text;
        }

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine($"UnityEngine.Debug.Log(\"[GEN]: {_text}\");");
        }

        public override bool IsAsync() => false;
    }
}