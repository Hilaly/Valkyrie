using System.Collections.Generic;
using Valkyrie.DSL.Definitions;

namespace Valkyrie.DSL.Actions
{
    class CreateTypeAction : IDslAction
    {
        public IStringProvider Name;
        public IStringProvider Type;

        public void Execute(Dictionary<string, string> args, CompilerContext context)
        {
            var type = context.GetOrCreateType(Name.GetString(args));
            type.TypeCategory = Type.GetString(args);
        }

        public override string ToString() => $"Create {Type} {Name}";
    }
}