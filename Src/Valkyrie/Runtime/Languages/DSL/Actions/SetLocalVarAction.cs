using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class SetLocalVarAction : IDslAction
    {
        public IStringProvider Name { get; set; }
        public IStringProvider Value { get; set; }

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var localArgs = lc.GetLocalVariables();
            var globalArgs = context.GlobalVariables;
            var name = Name.GetString(localArgs, globalArgs);
            var value = Value.GetString(localArgs, globalArgs);

            lc.SetValue(name, value);
        }
    }
}