using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class SetGlobalVarAction : IDslAction
    {
        public IStringProvider Name { get; set; }
        public IStringProvider Value { get; set; }

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var localArgs = lc.GetLocalVariables();
            var globalArgs = context.GlobalVariables;
            var name = Name.GetString(localArgs, globalArgs);
            var value = Value.GetString(localArgs, globalArgs);

            context.GlobalVariables[name] = value;
        }
    }
}