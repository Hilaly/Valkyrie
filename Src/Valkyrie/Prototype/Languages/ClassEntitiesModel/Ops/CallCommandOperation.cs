using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Ops
{
    class CallCommandOperation : EventHandlerOperation
    {
        private readonly string _command;
        private readonly List<string> _args;

        public CallCommandOperation(string command, string[] args)
        {
            _command = command;
            _args = new List<string>(args);
        }

        public override bool IsAsync() => true;

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine($"await Interpreter.Execute({_command}{_args.Select(x => $", {x}").Join(string.Empty)});");
        }
    }
}