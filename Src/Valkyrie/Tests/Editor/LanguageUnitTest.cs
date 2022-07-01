using System.Diagnostics;
using NUnit.Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Valkyrie.Language
{
    public class LanguageUnitTest
    {
        static int[] _iterations = { 1, 10, 100, 1000, 10000 };

        // A Test behaves as an ordinary method
        [Test]
        public void TestScriptProgram([ValueSource(nameof(_iterations))] int iterationsCount)
        {
            var fileName = "TestLogicProgram";
            var world = Constructor.Create();

            world.Compile(Resources.Load<TextAsset>(fileName).text);

            TestWorld(world, iterationsCount);

            Assert.AreEqual(3 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Height", 5f);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 3f);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 5f);
            Assert.AreEqual(5 * 5, world.GetFacts("Cell").All.Length);
            var cellsQuery = world.GetWorldQuery("Cells");
            Assert.AreEqual(25, cellsQuery.Request().Length);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void TestIterationsProgram()
        {
            var fileName = "TestIterationsProgram";
            var world = Constructor.Create();

            world.Compile(Resources.Load<TextAsset>(fileName).text);

            TestWorld(world, 3);

            Assert.AreEqual(3 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Height", 5f);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 3f);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 5f);
            Assert.AreEqual(5 * 5, world.GetFacts("Cell").All.Length);
            
            var cellsQuery = world.GetWorldQuery("Cells");
            Assert.AreEqual(25, cellsQuery.Request().Length);
        }

        [Test]
        public void TestCsCodeProgram([ValueSource(nameof(_iterations))] int iterationsCount)
        {
            var world = Constructor.Create();

            var factName = "John";
            Assert.AreEqual(factName, world.GetFactName(world.GetFactId(factName)));

            world.AddRule("Height", (w, fact) =>
            {
               //UnityEngine.Debug.Log($"Height=>Row {fact.ToString(w)}");
                w.TryAddFact("Row", fact[0]);
            });
            world.AddRule("Row", (w, fact) =>
            {
                //UnityEngine.Debug.Log($"Row=>Row {fact.ToString(w)}");
                if (fact[0].AsInt() > 1)
                    w.TryAddFact("Row", fact[0].AsInt() - 1);
            });
            world.AddRule("Width", (w, fact) =>
            {
                //UnityEngine.Debug.Log($"Width=>Column {fact.ToString(w)}");
                w.TryAddFact("Column", fact[0]);
            });
            world.AddRule("Column", (w, fact) =>
            {
                //UnityEngine.Debug.Log($"Column=>Column {fact.ToString(w)}");
                if (fact[0].AsInt() > 1)
                    w.TryAddFact("Column", fact[0].AsInt() - 1);
            });

            world.AddRule("Row", "Column", (w, row, column) =>
            {
                //UnityEngine.Debug.Log($"RC=>Cell {row.ToString(w)} {column.ToString(w)}");
                w.TryAddFact("Cell", w.Generate(), row[0], column[0]);
            });

            world.AddStartupRule(w =>
            {
                w.AddFact("Positive", 7);
                w.AddFact("Greater_than", 4, 2);
                w.AddFact("Direction", "west");
                w.AddFact("Direction", "east");
                w.AddFact("Player_symbol", "@");

                w.AddFact("Height", 3);
                w.PushFact("Width", 3);
            });

            TestWorld(world, iterationsCount);

            Assert.AreEqual(3 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Height", 5);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 3);
            Assert.AreEqual(5 * 3, world.GetFacts("Cell").All.Length);
            world.PushFact("Width", 5);
            Assert.AreEqual(5 * 5, world.GetFacts("Cell").All.Length);
            foreach (var fact in world.GetFacts("Cell").All)
            {
                Debug.Log(fact.ToString(world));
            }
        }

        void TestWorld(IWorld world, int iterationsCount)
        {
            var st = new Stopwatch();
            st.Start();
            for (var i = 0; i < iterationsCount; ++i)
                world.Init();
            st.Stop();
            Debug.Log($"Time={st.Elapsed.TotalMilliseconds / iterationsCount:F4} ms");
        }
    }
}