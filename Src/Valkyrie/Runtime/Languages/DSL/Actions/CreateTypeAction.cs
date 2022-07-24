using System.Collections.Generic;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class CreateTypeAction : IDslAction
    {
        public IStringProvider Name;
        public IStringProvider Type;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetArgs();
            var type = context.GetOrCreateType(Name.GetString(args));
            type.TypeCategory = Type.GetString(args);
        }

        public override string ToString() => $"Create {Type} {Name}";
    }
}