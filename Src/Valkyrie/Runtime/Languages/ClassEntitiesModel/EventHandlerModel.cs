using System;
using System.Linq;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie
{
    public class EventHandlerModel : MethodImpl
    {
        public readonly EventEntity Event;
        private readonly string _uid;

        public EventHandlerModel(EventEntity @event)
        {
            _uid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            Event = @event;
        }

        public void Write(FormatWriter sb)
        {
            bool isAsync = _ops.Any(x => x.IsAsync());
            sb.BeginBlock(
                $"{(isAsync ? "async " : string.Empty)}System.Threading.Tasks.Task {GetMethodName()}({Event.ClassName} ev)");
            foreach (var op in _ops)
                op.Write(sb, OpType.Handler);
            if (!isAsync)
                sb.AppendLine("return System.Threading.Tasks.Task.CompletedTask;");
            sb.EndBlock();
        }

        public string GetMethodName()
        {
            return $"On{Event.ClassName}Handle{_uid}";
        }
    }
}