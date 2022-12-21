using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Language.Language;

namespace Valkyrie.Language.Ecs
{
    class World : DataHolder, IWorld
    {
        private readonly List<IRule> _rules = new List<IRule>();
        private readonly List<IRule> _startupRules = new List<IRule>();

        private readonly Dictionary<string, IWorldQuery> _worldQueries = new Dictionary<string, IWorldQuery>();

        #region Facts

        public void PushFact(Fact fact)
        {
            AddFact(fact, false);
            TriggerEvents();
        }

        public void AddFact(Fact fact, bool skipIfExist)
        {
            var factId = fact.Id;
            var pool = (Pool)GetPool(factId);
            pool.Add(fact, skipIfExist);
        }

        public IFactsPool GetFacts(int factId) => GetPool(factId);

        #endregion

        #region ILogic

        public void AddRule(IRule rule)
        {
            _rules.Add(rule);
        }

        public void AddStartupRule(IRule rule)
        {
            _startupRules.Add(rule);
        }

        public void Init()
        {
            Reset();

            for (var index = 0; index < _startupRules.Count; index++)
            {
                var rule = _startupRules[index];
                rule.Run(this, new List<int>());
            }

            TriggerEvents();
        }

        public void Simulate()
        {
            foreach (var rule in _rules)
            {
                FetchPools(new List<int>());
                rule.RunAll(this);
            }
        }

        #endregion

        #region IRequestApi

        public IWorldQuery GetWorldQuery(string name) =>
            _worldQueries.TryGetValue(name, out var result) ? result : default;

        public void AddWorldQuery(string name, IWorldQuery worldQuery) => _worldQueries[name] = worldQuery;

        #endregion

        void TriggerEvents()
        {
            var changedTypes = new List<int>();
            var iterations = 0;
            while (iterations < 100)
            {
                changedTypes.Clear();
                FetchPools(changedTypes);
                if (changedTypes.Count == 0)
                    break;

                //Debug.Log($"Trigger event {string.Join(",", changedTypes.Select(GetFactName))} {string.Join(",", GetChangedFacts().Select(x => x.ToString(this)))}");
                foreach (var rule in _rules)
                    if (rule.IsDependsOn(changedTypes))
                        rule.Run(this, changedTypes);
                iterations++;
            }
        }
    }
}