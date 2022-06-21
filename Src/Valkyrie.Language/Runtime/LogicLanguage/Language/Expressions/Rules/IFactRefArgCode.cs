using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    interface IFactRefArgCode
    {
        Variable TryExecute(IWorld world, Fact fact, Variable[] localVariable);
    }
}