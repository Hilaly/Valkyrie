using System.Collections.Generic;
using Valkyrie.DSL.Definitions;

namespace Valkyrie.DSL.Actions
{
    class AddbaseTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider BaseType;

        public void Execute(Dictionary<string, string> args, CompilerContext context)
        {
            var type = context.GetOrCreateType(Type.GetString(args));
            type.BaseTypes.Add(BaseType.GetString(args));
        }

        public override string ToString() => $"{Type} inherited from {BaseType}";
    }
}