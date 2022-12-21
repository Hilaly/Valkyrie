using System.Collections.Generic;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class CreateTypeAction : IDslAction
    {
        public IStringProvider Name;
        public IStringProvider Type;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetLocalVariables();
            var type = context.GetOrCreateType(Name.GetString(args, context.GlobalVariables));
            type.TypeCategory = Type.GetString(args, context.GlobalVariables);
        }

        public override string ToString() => $"Create {Type} {Name}";
    }
}