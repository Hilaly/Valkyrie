using System.Collections.Generic;
using Valkyrie.Language.Language.Compiler;
using Valkyrie.Language.Language.Expressions.Facts;

namespace Valkyrie.Language.Language
{
    class StartupScriptRule : IRule
    {
        private readonly List<GenerateFactRuntimeMethod> _factsConstructors;
        private readonly LocalVarsDesc _localVariables;

        public StartupScriptRule(List<GenerateFactRuntimeMethod> list, LocalVarsDesc localVariables)
        {
            _factsConstructors = list;
            _localVariables = localVariables;
        }

        public bool IsDependsOn(List<int> factId) => false;

        public void Run(IWorld world, List<int> changedTypes)
        {
            var buffer = _localVariables.GetBuffer();
            for (var i = 0; i < _factsConstructors.Count; ++i)
                _factsConstructors[i].Run(world, buffer);
        }

        public void RunAll(IWorld world)
        {
            var buffer = _localVariables.GetBuffer();
            for (var i = 0; i < _factsConstructors.Count; ++i)
                _factsConstructors[i].Run(world, buffer);
        }
    }
}