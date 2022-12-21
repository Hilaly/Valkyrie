using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace Valkyrie.Language.Ecs
{
    class Pool : IFactsPool
    {
        private readonly List<Fact> _facts = new List<Fact>();
        private readonly List<Fact> _toAdd = new List<Fact>();
        private readonly List<Fact> _changed = new List<Fact>();
        private readonly List<Fact> _nonChanged = new List<Fact>();
        private readonly HashSet<int> _toRemove = new HashSet<int>();

        public Span<Fact> All => new Span<Fact>(_facts.ToArray());
        public Span<Fact> Changed => new Span<Fact>(_changed.ToArray());
        public Span<Fact> NonChanged => new Span<Fact>(_nonChanged.ToArray());

        public void Add(Fact c, bool skipIfExist)
        {
            //Remove old, if exist
            for (var index = 0; index < _facts.Count; index++)
            {
                var fact = _facts[index];
                if (fact[0] == c[0])
                {
                    if(skipIfExist && fact == c)
                        return;
                    _toRemove.Add(index);
                }
            }
            
            //Replace new, if exist
            for (var index = 0; index < _toAdd.Count; index++)
            {
                var fact = _toAdd[index];
                if (fact[0] == c[0])
                {
                    _toAdd[index] = c;
                    return;
                }
            }

            //Just add as new
            _toAdd.Add(c);
        }

        public bool Fetch()
        {
            var wasAdded = _toAdd.Count > 0 || _toRemove.Count > 0;

            foreach (var index in _toRemove.OrderByDescending(x => x))
                _facts.RemoveAtWithReplace(index);
            _toRemove.Clear();

            _nonChanged.Clear();
            _nonChanged.AddRange(_facts);
            
            _changed.Clear();
            _changed.AddRange(_toAdd);
            
            _facts.AddRange(_toAdd);
            
            _toAdd.Clear();

            return wasAdded;
        }

        public void Clear()
        {
            _facts.Clear();
            _toAdd.Clear();
            _changed.Clear();
            _nonChanged.Clear();
        }
    }
}