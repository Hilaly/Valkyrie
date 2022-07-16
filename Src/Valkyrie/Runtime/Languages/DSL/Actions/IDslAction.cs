using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.DSL.Actions
{
    interface IDslAction
    {
        void Execute(Dictionary<string,string> args, CompilerContext context);
    }

    class SkipAction : IDslAction
    {
        public override string ToString() => "skip";
        public void Execute(Dictionary<string, string> args, CompilerContext context) => Debug.LogWarning($"skip");
    }

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