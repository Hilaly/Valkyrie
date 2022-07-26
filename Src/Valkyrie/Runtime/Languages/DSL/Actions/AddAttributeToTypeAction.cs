using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class AddAttributeToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Attribute;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetArgs();
            var type = context.GetOrCreateType(Type.GetString(args));
            type.Attributes.Add(Attribute.GetString(args));
        }

        public override string ToString() => $"{Type} has attribute {Attribute}";
    }
}