using System.Collections.Generic;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class SequenceAction : IDslAction
    {
        private List<IDslAction> _actions;

        public SequenceAction(List<IDslAction> actions)
        {
            _actions = actions;
        }

        public void Execute(LocalContext localContext, CompilerContext context)
        {
            _actions.ForEach(action => action.Execute(localContext, context));
        }
    }
}