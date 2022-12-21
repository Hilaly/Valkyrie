using System.Collections.Generic;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddBaseTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider BaseType;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            type.AddBase(BaseType.GetString(args, context.GlobalVariables));
        }

        public override string ToString() => $"{Type} inherited from {BaseType}";
    }
}