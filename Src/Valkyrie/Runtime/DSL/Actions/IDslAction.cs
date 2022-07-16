using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Ecs.DSL;

namespace DSL.Actions
{
    interface IDslAction
    {
        void Execute(Dictionary<string,string> args, CompilerContext context);
    }

    class SkipAction : IDslAction
    {
        public override string ToString() => "skip";
        public void Execute(Dictionary<string, string> args, CompilerContext context)
        {
            Debug.LogWarning($"skip");
        }
    }

    class CreateTypeAction : IDslAction
    {
        public IStringProvider Name;
        public IStringProvider Type;
        
        public void Execute(Dictionary<string, string> args, CompilerContext context)
        {
            Debug.LogWarning($"Will create: {Type.GetString(args)} {Name.GetString(args)}");
        }

        public override string ToString()
        {
            return $"Create {Type} {Name}";
        }
    }
}