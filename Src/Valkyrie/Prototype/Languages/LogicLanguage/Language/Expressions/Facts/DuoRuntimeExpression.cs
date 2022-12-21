using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    abstract class DuoRuntimeExpression : IRuntimeExpression
    {
        private readonly IRuntimeExpression _first;
        private readonly IRuntimeExpression _second;

        public bool IsIgnoredOnCompare => _first.IsIgnoredOnCompare || _second.IsIgnoredOnCompare;

        protected DuoRuntimeExpression(IRuntimeExpression first, IRuntimeExpression second)
        {
            _first = first;
            _second = second;
        }

        public Variable Run(IWorld world, Variable[] localVariables) =>
            new Variable(DoOp(
                _first.Run(world, localVariables).AsFloat(),
                _second.Run(world, localVariables).AsFloat()
            ));

        protected abstract float DoOp(float f, float s);
    }
}