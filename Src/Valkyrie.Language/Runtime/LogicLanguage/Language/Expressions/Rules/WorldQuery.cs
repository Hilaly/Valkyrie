using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language.Compiler;
using Valkyrie.Language.Language.Expressions.Facts;

namespace Valkyrie.Language.Language.Expressions.Rules
{
    class WorldQueryResult : IWorldQueryResult
    {
        private readonly LocalVarsDesc _localVariables;
        private readonly Variable[] _buffer;

        public WorldQueryResult(LocalVarsDesc localVariables)
        {
            _localVariables = localVariables;
            _buffer = new Variable[_localVariables.Count];
        }

        public Variable this[string name] => this[_localVariables.GetId(name)];
        public Variable this[int index] => _buffer[index];

        public int Count => _localVariables.Count;

        public Variable[] GetBuffer() => _buffer;
        public IEnumerator<Variable> GetEnumerator()
        {
            for (var index = 0; index < _buffer.Length; index++)
                yield return _buffer[index];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    class WorldQuery : IWorldQuery
    {
        private readonly List<int> _dependsOnFacts;
        private readonly LocalVarsDesc _localVarsDesc;
        private readonly List<IFactIdProvider> _expressions;
        private readonly List<IRuntimeExpression> _conclusions;
        private readonly List<int> _factsCollections;
        private readonly Fact[] _factsBuffer;

        private readonly List<IWorldQueryResult> _results = new List<IWorldQueryResult>();
        private readonly IWorld _world;

        public WorldQuery(LocalVarsDesc localVariables, List<IFactIdProvider> expressions,
            List<IRuntimeExpression> conclusions, IWorld world)
        {
            _localVarsDesc = localVariables;
            _expressions = expressions;
            _conclusions = conclusions;
            _world = world;
            _factsBuffer = new Fact[expressions.Count];
            _factsCollections = expressions.ConvertAll(x => x.FactId);
            _dependsOnFacts = _factsCollections
                .Where(x => x >= 0)
                .ToHashSet()
                .ToList();
        }

        public void Run(IWorld world, List<int> changedTypes)
        {
            _results.Clear();
            world.IterateAll(_factsCollections, _factsBuffer, facts =>
            {
                var queryResult = new WorldQueryResult(_localVarsDesc);
                var buffer = queryResult.GetBuffer();
                
                for (var i = 0; i < _expressions.Count; ++i)
                {
                    var expression = _expressions[i];
                    var fact = facts[i];
                    if (!expression.Check(world, fact, buffer).AsBool())
                        return;
                }

                //TODO: Create result collection
                for (var i = 0; i < _conclusions.Count; ++i)
                    _conclusions[i].Run(world, buffer);
                _results.Add(queryResult);
            });
        }

        public Span<IWorldQueryResult> Request()
        {
            Run(_world, _dependsOnFacts);
            return new Span<IWorldQueryResult>(_results.ToArray());
        }
    }
}