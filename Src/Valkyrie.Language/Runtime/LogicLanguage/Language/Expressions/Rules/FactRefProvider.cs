using System.Collections.Generic;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class FactRefProvider : IFactIdProvider, IRuntimeCheck
    {
        private readonly List<IFactRefArgCode> _args;
        public int FactId { get; }
        
        public IFactIdProvider NextProvider { get; set; }

        public FactRefProvider(int factId, List<IFactRefArgCode> args)
        {
            _args = args;
            FactId = factId;
        }

        public Variable Check(IWorld world, Fact fact, Variable[] localVariables)
        {
            for (var i = 0; i < _args.Count; ++i)
            {
                var code = _args[i];
                var r = code.TryExecute(world, fact, localVariables);
                if (r.IsBool() && r.AsBool() == false)
                    return Variable.False;
            }

            return Variable.True;
        }
    }
}