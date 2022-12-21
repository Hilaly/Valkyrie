using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddPropertyToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Property;
        
        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var args = localContext.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            type.GetOrCreateProperty(Property.GetString(args, context.GlobalVariables));
        }

        public override string ToString() => $"{Type} has property {Property}";
    }
}