using System.Collections.Generic;
using Valkyrie.Language.Ecs;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    public interface IStartupAction
    {
        IEnumerable<Fact> Run(IWorld world);
    }
}