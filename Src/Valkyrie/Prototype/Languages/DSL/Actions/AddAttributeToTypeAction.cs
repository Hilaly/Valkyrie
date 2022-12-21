using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddAttributeToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Attribute;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            type.AddAttribute(Attribute.GetString(args, context.GlobalVariables));
        }

        public override string ToString() => $"{Type} has attribute {Attribute}";
    }
}