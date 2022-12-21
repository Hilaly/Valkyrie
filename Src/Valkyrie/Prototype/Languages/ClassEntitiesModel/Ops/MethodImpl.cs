using System.Collections.Generic;
using Valkyrie.Ops;

namespace Valkyrie
{
    public class MethodImpl
    {
        protected readonly List<EventHandlerOperation> _ops = new();

        public MethodImpl LogOp(string text)
        {
            _ops.Add(new LogOperation(text));
            return this;
        }

        public MethodImpl RaiseOp(EventEntity raisedEvent, params string[] args)
        {
            _ops.Add(new RaiseEventOperation(raisedEvent, args));
            return this;
        }

        public MethodImpl CommandOp(string command, params string[] args)
        {
            _ops.Add(new CallCommandOperation(command, args));
            return this;
        }

        public MethodImpl CodeOp(string code)
        {
            _ops.Add(new WriteCodeLine(code));
            return this;
        }
    }
}