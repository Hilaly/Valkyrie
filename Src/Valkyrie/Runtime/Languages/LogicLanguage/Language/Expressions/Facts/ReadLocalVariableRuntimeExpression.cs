using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    class ReadLocalVariableRuntimeExpression : IRuntimeExpression
    {
        private readonly int _index;

        public bool IsIgnoredOnCompare => false;

        public ReadLocalVariableRuntimeExpression(int index)
        {
            _index = index;
        }

        public Variable Run(IWorld world, Variable[] localVariables) => localVariables[_index];
    }
}