using System.Collections.Generic;

namespace Valkyrie.Language.Language
{
    public interface IRule
    {
        bool IsDependsOn(List<int> factId);
        
        void Run(IWorld world, List<int> changedTypes);
        void RunAll(IWorld world);
    }
}