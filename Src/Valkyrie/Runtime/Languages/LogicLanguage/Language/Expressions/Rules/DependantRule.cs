using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language.Compiler;
using Valkyrie.Language.Language.Expressions.Facts;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class DependantRule : IRule
    {
        private readonly List<int> _dependsOnFacts;
        private readonly LocalVarsDesc _localVarsDesc;
        private readonly List<IFactIdProvider> _expressions;
        private readonly List<int> _factsCollections;
        private readonly Fact[] _factsBuffer;
        
        private readonly List<GenerateFactRuntimeMethod> _factsConstructors;

        public bool IsDependsOn(List<int> factId) => factId.Any(x => _dependsOnFacts.Contains(x));

        public DependantRule(List<GenerateFactRuntimeMethod> list, LocalVarsDesc localVariables, List<IFactIdProvider> expressions)
        {
            _localVarsDesc = localVariables;
            _factsConstructors = list;
            _expressions = expressions;
            _factsBuffer = new Fact[expressions.Count];
            _factsCollections = expressions.ConvertAll(x => x.FactId);
            _dependsOnFacts = _factsCollections
                .Where(x => x >= 0)
                .ToHashSet()
                .ToList();
        }

        public void Run(IWorld world, List<int> changedTypes)
        {
            world.Iterate(changedTypes, _factsCollections, _factsBuffer, fact => ProcessCall(fact, world));
        }

        public void RunAll(IWorld world)
        {
            world.IterateAll(_factsCollections, _factsBuffer, fact => ProcessCall(fact, world));
        }
        
        void ProcessCall(Fact[] facts, IWorld w)
        {
            var buffer = _localVarsDesc.GetBuffer();

            for (var i = 0; i < _expressions.Count; ++i)
            {
                var expression = _expressions[i];
                var fact = facts[i];
                if (!expression.Check(w, fact, buffer).AsBool()) return;
            }

            for (var i = 0; i < _factsConstructors.Count; ++i) _factsConstructors[i].Run(w, buffer);
        }

    }
}