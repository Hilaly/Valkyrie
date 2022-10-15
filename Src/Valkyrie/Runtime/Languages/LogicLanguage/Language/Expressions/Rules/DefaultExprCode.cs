using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    abstract class DefaultExprCode : IFactIdProvider, IFactRefArgCode, IRuntimeCheck
    {
        public int FactId => -1;
        
        public IFactIdProvider NextProvider { get; set; }

        public abstract Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable);
        public Variable Check(IWorld world, Fact fact, Variable[] localVariables) => TryExecute(world, fact, localVariables);

    }
}