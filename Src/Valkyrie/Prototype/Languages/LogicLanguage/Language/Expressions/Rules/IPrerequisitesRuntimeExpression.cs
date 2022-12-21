using System.Collections.Generic;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    public interface IPrerequisitesRuntimeExpression
    {
        List<Fact> Run(IWorld world);
        bool DependsOn(int factId);
        List<int> Process(IWorld world);
    }
}