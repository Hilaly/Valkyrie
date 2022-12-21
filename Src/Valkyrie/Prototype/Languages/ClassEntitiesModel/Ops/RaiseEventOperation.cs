using System;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.Ops
{
    class RaiseEventOperation : EventHandlerOperation
    {
        private readonly EventEntity _raisedEvent;
        private readonly string[] _args;

        public RaiseEventOperation(EventEntity raisedEvent, string[] args)
        {
            _raisedEvent = raisedEvent;
            _args = args;

            if (_raisedEvent.Args.Count != args.Length)
                throw new Exception($"To raise {raisedEvent.Name} need to use {raisedEvent.Args.Count} args");
        }

        public override bool IsAsync() => true;

        public override void Write(FormatWriter sb, OpType opType)
        {
            var args = string.Empty;
            for (var i = 0; i < _args.Length; ++i)
                args += $", Arg{i} = {_args[i]}";
            if (args.Length > 0)
                args = args[2..];
            sb.AppendLine($"await Raise(new {_raisedEvent.ClassName} {{ {args} }});");
        }
    }
}