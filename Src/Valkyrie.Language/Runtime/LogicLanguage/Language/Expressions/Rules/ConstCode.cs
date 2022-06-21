using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class ConstCode : DefaultExprCode
    {
        private readonly Variable _constValue;

        public ConstCode(Variable constValue)
        {
            _constValue = constValue;
        }

        public override Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable) => _constValue;
    }
}