using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class SetPropertyTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Property;
        public IStringProvider Code;
        
        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var args = localContext.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            var prop = type.GetOrCreateProperty(Property.GetString(args, context.GlobalVariables));
            prop.Type = Code.GetString(args, context.GlobalVariables);
        }
    }
}