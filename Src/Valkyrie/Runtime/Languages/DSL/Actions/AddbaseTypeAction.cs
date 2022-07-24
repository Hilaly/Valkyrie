using System.Collections.Generic;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class AddBaseTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider BaseType;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetArgs();
            var type = context.GetOrCreateType(Type.GetString(args));
            type.BaseTypes.Add(BaseType.GetString(args));
        }

        public override string ToString() => $"{Type} inherited from {BaseType}";
    }
}