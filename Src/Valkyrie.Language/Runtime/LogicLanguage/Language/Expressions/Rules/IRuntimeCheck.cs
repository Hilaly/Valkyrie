using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    interface IRuntimeCheck
    {
        Variable Check(IWorld world, Fact fact, Variable[] localVariables);
    }
}