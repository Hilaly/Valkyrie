using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language;
using Valkyrie.Language.Language.Compiler;

namespace Valkyrie.Language
{
    public static class LanguageExtension
    {
        #region Facts

        public static IEnumerable<Fact> CollectAllFacts(this IWorld world)
        {
            return world is World w ? w.GetAllFacts() : Enumerable.Empty<Fact>();
        }

        public static void AddFact(this IWorld world, string factName, params Variable[] args)
        {
            var factId = world.GetFactId(factName);
            var fact = new Fact(factId, args);
            world.AddFact(fact, false);
        }

        public static void PushFact(this IWorld world, string factName, params Variable[] args)
        {
            var factId = world.GetFactId(factName);
            var fact = new Fact(factId, args);
            world.PushFact(fact);
        }

        public static void TryAddFact(this IWorld world, string factName, params Variable[] args) => 
            TryAddFact(world, world.GetFactId(factName), 1 << 0, args);

        public static void TryAddFact(this IWorld world, int factId, byte ignored, params Variable[] args)
        {
            var fact = new Fact(factId, args) { };
            world.AddFact(fact, true);
        }

        public static IFactsPool GetFacts(this IWorld world, string name) =>
            world.GetFacts(world.GetFactId(name));

        #endregion

        #region Rules

        public static void AddRule(this IWorld world, Action<IWorld, List<int>> ruleAction,
            params string[] depends) =>
            world.AddRule(new ActionRule(ruleAction, depends.Select(world.GetFactId).ToArray()));

        public static void AddRule(this IWorld world, string factName, Action<IWorld, Fact> ruleAction)
        {
            world.AddRule((w, ct) => w.Iterate(ct, factName, fact => ruleAction(w, fact)), factName);
        }

        public static void AddRule(this IWorld world, string fact1Name, string fact2Name,
            Action<IWorld, Fact, Fact> ruleAction) =>
            world.AddRule((w, ct) => w.Iterate(ct, fact1Name, fact2Name, (f1, f2) => ruleAction(world, f1, f2)),
                fact1Name, fact2Name);

        public static void AddStartupRule(this IWorld world, Action<IWorld> call) =>
            world.AddStartupRule(new StartupActionRule(call));

        #endregion

        #region Iterate

        public static void Iterate(this IWorld world, List<int> changed, string factName, Action<Fact> call) =>
            IterateAllWithChangeCheck(world, changed, 
                new List<int>()
                {
                    world.GetFactId(factName)
                }, new Fact[1], (list) => call(list[0]));

        public static void Iterate(this IWorld world, List<int> changed, string fact1Name, string fact2Name,
            Action<Fact, Fact> call) =>
            IterateAllWithChangeCheck(world, changed,
                new List<int>()
                {
                    world.GetFactId(fact1Name), 
                    world.GetFactId(fact2Name)
                }, new Fact[2], (list) => call(list[0], list[1]));

        public static void Iterate(this IWorld world, List<int> changed, string fact1Name, string fact2Name, string fact3Name,
            Action<Fact, Fact, Fact> call) =>
            IterateAllWithChangeCheck(world, changed,
                new List<int>()
                {
                    world.GetFactId(fact1Name), 
                    world.GetFactId(fact2Name), 
                    world.GetFactId(fact3Name)
                }, new Fact[3], (list) => call(list[0], list[1], list[2]));

        internal static void Iterate(this IWorld world, List<int> changed, List<int> factIds, Fact[] buffer,
            Action<Fact[]> call)
        {
            if(factIds.Count == 0)
                return;
            IterateAllWithChangeCheck(world, changed, factIds, buffer, call);
        }

        internal static void IterateAllWithChangeCheck(this IWorld world, List<int> changed, List<int> factIds,
            Fact[] buffer, Action<Fact[]> call)
        {
            //Debug.Log($"======START=========");
            var factsCount = factIds.Count;

            var listChanged = new List<bool>(factsCount);
            var listIndices = new List<int>(factsCount);
            var listFacts = new List<Fact[]>(factsCount);
            for (var i = 0; i < factsCount; ++i)
            {
                listIndices.Add(0);
                listFacts.Add(null);
                listChanged.Add(changed.Contains(factIds[i]));
            }
            
            Assert.AreEqual(factsCount, factIds.Count);
            Assert.AreEqual(factsCount, buffer.Length);
            Assert.AreEqual(factsCount, listChanged.Count);
            Assert.AreEqual(factsCount, listIndices.Count);
            Assert.AreEqual(factsCount, listFacts.Count);

            void FillList(int listIndex, bool needChanged)
            {
                var pool = world.GetFacts(factIds[listIndex]);
                listFacts[listIndex] = (needChanged ? pool.Changed : pool.NonChanged).ToArray();
            }

            void IterateLists()
            {
                while (true)
                {
                    //Debug.Log($"Checking {string.Join(",", listIndices)}");
                    for (var i = 0; i < factsCount; ++i)
                    {
                        var factIndex = listIndices[i];
                        var facts = listFacts[i];
                        buffer[i] = facts[factIndex];
                    }

                    call(buffer);

                    var wasAdvanced = false;
                    for (var i = 0; i < factsCount; ++i)
                    {
                        var facts = listFacts[i];
                        var maxIndex = facts.Length;
                        listIndices[i] += 1;
                        if (listIndices[i] < maxIndex)
                        {
                            wasAdvanced = true;
                            break;
                        }

                        listIndices[i] = 0;
                    }

                    if (!wasAdvanced)
                        break;
                }

                //Debug.Log($"Checking finished");
            }

            void FillChangedForAndIterate(List<int> changedIndices)
            {
                //Debug.Log($"FillChangedForAndIterate: {string.Join(",", changedIndices)}");
                for (var indexToFill = 0; indexToFill < factsCount; ++indexToFill)
                {
                    FillList(indexToFill, changedIndices.Contains(indexToFill));
                    if(listFacts[indexToFill].Length == 0)
                        return;
                }
                IterateLists();
            }

            void IterateFor(int changedCount)
            {
                //Debug.Log($"Iterate {changedCount} count");
                var s = changedCount - 1;

                var indices = new List<int>(changedCount);
                for (var i = 0; i < changedCount; ++i) 
                    indices.Add(i);

                //Debug.Log($"fill indices {string.Join(",", indices)}");
                
                while (true)
                {
                    var allChanged = true;
                    for (var i = 0; i < changedCount; ++i)
                    {
                        var index = indices[i];
                        if (!listChanged[index])
                        {
                            allChanged = false;
                            break;
                        }
                    }

                    if (allChanged) 
                        FillChangedForAndIterate(indices);

                    var wasAdvanced = false;
                    for (var i = changedCount - 1; i >= 0; --i)
                    {
                        indices[i] += 1;
                        var maxValue = factsCount - s + i;
                        if (indices[i] < maxValue)
                        {
                            for (var j = i + 1; j < changedCount; ++j)
                            {
                                indices[j] = indices[j - 1] + 1;
                            }
                            //Debug.Log($"fill indices {string.Join(",", indices)}");
                            wasAdvanced = true;
                            break;
                        }
                    }
                    
                    if (!wasAdvanced)
                        break;
                }
            }

            for(var i = 1; i <= factsCount; ++i)
                IterateFor(i);
            //Debug.Log($"======Finish========");
        }

        internal static void IterateAll(this IWorld world, List<int> factIds, Fact[] buffer, Action<Fact[]> call)
        {
            var factsCount = factIds.Count;

            var listIndices = new List<int>(factsCount);
            var listFacts = new List<Fact[]>(factsCount);
            for (var i = 0; i < factsCount; ++i)
            {
                listIndices.Add(0);
                var facts = world.GetFacts(factIds[i]).All.ToArray();
                if(facts.Length == 0)
                    return;
                listFacts.Add(facts);
            }


            while (true)
            {
                //Debug.Log($"Checking {string.Join(",", listIndices)}");
                for (var i = 0; i < factsCount; ++i)
                {
                    var factIndex = listIndices[i];
                    var facts = listFacts[i];
                    buffer[i] = facts[factIndex];
                }

                call(buffer);

                var wasAdvanced = false;
                for (var i = 0; i < factsCount; ++i)
                {
                    var facts = listFacts[i];
                    var maxIndex = facts.Length;
                    listIndices[i] += 1;
                    if (listIndices[i] < maxIndex)
                    {
                        wasAdvanced = true;
                        break;
                    }

                    listIndices[i] = 0;
                }

                if (!wasAdvanced)
                    break;
            }

            //Debug.Log($"Checking finished");
        }

        #endregion

        #region world

        public static void Compile(this IWorld world, string txtProgram)
        {
            var ast = LanguageAstConstructor.Parse(txtProgram);
            LanguageCompiler.Compile(world, ast, string.Empty);
        }

        #endregion
    }
}