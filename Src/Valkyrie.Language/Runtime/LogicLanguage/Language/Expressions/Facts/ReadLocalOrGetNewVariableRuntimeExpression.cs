using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    class ReadLocalOrGetNewVariableRuntimeExpression : IRuntimeExpression
    {
        private readonly int _index;

        public bool IsIgnoredOnCompare => true;

        public ReadLocalOrGetNewVariableRuntimeExpression(int index)
        {
            _index = index;
        }

        public Variable Run(IWorld world, Variable[] localVariables)
        {
            return localVariables[_index] = new Variable(world.Generate());
        }
    }
}