using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    class ConstRuntimeExpression : IRuntimeExpression
    {
        public bool IsIgnoredOnCompare => false;
        private readonly Variable _value;
        public ConstRuntimeExpression(Variable value)
        {
            _value = value;
        }
        public Variable Run(IWorld world, Variable[] localVariables) => _value;
    }
}