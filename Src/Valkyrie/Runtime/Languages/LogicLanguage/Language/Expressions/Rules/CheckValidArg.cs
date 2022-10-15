using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language.Expressions.Facts;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class CheckValidArg : IFactRefArgCode
    {
        private readonly int _factArgIndex;
        private readonly IRuntimeExpression _expression;

        public CheckValidArg(int factArgIndex, IRuntimeExpression expression)
        {
            _factArgIndex = factArgIndex;
            _expression = expression;
        }

        public Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable)
        {
            var targetValue = _expression.Run(world, localVariable);
            var factArg = fact[_factArgIndex];
            if (factArg == targetValue)
                return new Variable(true);
            return new Variable(false);
        }
    }
}