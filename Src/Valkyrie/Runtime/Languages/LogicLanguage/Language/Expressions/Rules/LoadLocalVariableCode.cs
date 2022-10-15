using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class LoadLocalVariableCode : DefaultExprCode
    {
        private readonly int _localVarIndex;

        public LoadLocalVariableCode(int localVarIndex)
        {
            _localVarIndex = localVarIndex;
        }

        public override Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable) => localVariable[_localVarIndex];
    }
}