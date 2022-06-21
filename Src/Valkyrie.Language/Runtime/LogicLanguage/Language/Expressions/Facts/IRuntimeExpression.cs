using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Facts
{
    public interface IRuntimeExpression
    {
        Variable Run(IWorld world, Variable[] localVariables);
        bool IsIgnoredOnCompare { get; }
    }
}