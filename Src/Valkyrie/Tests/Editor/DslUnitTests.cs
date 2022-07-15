using NUnit.Framework;
using UnityEngine;
using Valkyrie.Ecs.DSL;

namespace Valkyrie.Language
{
    public class DslUnitTests
    {
        private static IDslDictionary LoadTestDictionary()
        {
            var dictionary = new DslCompiler().Dictionary;
            var source = Resources.Load<TextAsset>("TestDictionary").text;
            dictionary.Load(source);
            return dictionary;
        }

        [Test]
        public void TestDictionaryLoad()
        {
            var dictionary = LoadTestDictionary();

            Debug.LogWarning(dictionary.ToString());
        }

        [Test]
        public void TestEntriesParse()
        {
            var dictionary = LoadTestDictionary();

            var localContext = new LocalContext();

            Assert.AreEqual(false, TryParseText("", dictionary, localContext));
            Assert.AreEqual(0, localContext.Args.Count);

            Assert.AreEqual(true, TryParseText("ASD<is flag>", dictionary, localContext));
            Assert.AreEqual("ASD", localContext.Args["name"]);
            Assert.AreEqual(true, TryParseText("GDB<is flag>", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.Args["name"]);
            
            Assert.AreEqual(true, TryParseText("GDB<is>ADB", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.Args["name"]);
            Assert.AreEqual("ADB", localContext.Args["component"]);
        }

        bool TryParseText(string text, IDslDictionary dictionary, LocalContext localContext)
        {
            foreach (var entry in dictionary.GetEntries)
                if (entry.TryMatch(text, localContext))
                    return true;
            return false;
        }

        [Test]
        public void TestParseProgram()
        {
            var compiler = new DslCompiler();
            var source = Resources.Load<TextAsset>("TestDslProgram").text;
            var ast = compiler.Build(source);
            Assert.NotNull(ast);
        }
    }
}