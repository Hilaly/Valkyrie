using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Language.Ecs
{
    class EmptyPool : IFactsPool
    {
        private Fact[] _collection = new[] { Fact.Empty };
        public Span<Fact> All => new Span<Fact>(_collection);
        public Span<Fact> Changed => new Span<Fact>(_collection);
        public Span<Fact> NonChanged => new Span<Fact>(_collection);
    }

    class DataHolder : IDataProvider
    {
        private readonly StringToIntConverter _factsConverter = new StringToIntConverter();
        private readonly List<Pool> _facts = new List<Pool>();
        private EmptyPool _emptyPool = new EmptyPool();

        private int _counter = 1;

        public int GetFactId(string factName)
        {
            var result = _factsConverter.GetId(factName);
            while (_facts.Count < _factsConverter.Count)
                _facts.Add(new Pool());
            return result;
        }

        public string GetFactName(int factId) => _factsConverter.GetString(factId);

        protected IFactsPool GetPool(int poolIndex) => poolIndex < 0 ? (IFactsPool)_emptyPool : _facts[poolIndex];
        public int Generate() => _counter++;

        protected void FetchPools(List<int> changed)
        {
            for (var i = 0; i < _facts.Count; ++i)
            {
                if (_facts[i].Fetch())
                    changed.Add(i);
            }
        }

        public IEnumerable<Fact> GetAllFacts() => _facts.SelectMany(x => x.All.ToArray());
        public IEnumerable<Fact> GetChangedFacts() => _facts.SelectMany(x => x.Changed.ToArray());

        protected void Reset()
        {
            foreach (var pool in _facts) 
                pool.Clear();
        }
    }
}