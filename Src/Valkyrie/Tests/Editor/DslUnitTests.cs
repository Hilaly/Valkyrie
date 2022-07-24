using System.IO;
using NUnit.Framework;
using UnityEngine;
using Valkyrie.DSL;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.Tools;

namespace Valkyrie.Language
{
    public class DslUnitTests
    {
        private static IDslDictionary LoadTestDictionary()
        {
            var dictionary = new DslCompiler().Dictionary;
            dictionary.Load(Resources.Load<TextAsset>("TestDictionary").text);

            Debug.LogWarning(dictionary);
            return dictionary;
        }

        [Test]
        public void TestDictionaryLoad()
        {
            var dictionary = LoadTestDictionary();

            Debug.LogWarning(dictionary);
        }

        [Test]
        public void TestEntriesParse()
        {
            var dictionary = LoadTestDictionary();

            var localContext = new LocalContext();

            Assert.AreEqual(false, TryParseText("", dictionary, localContext));
            Assert.AreEqual(0, localContext.Args.Count);

            Assert.AreEqual(true, TryParseText("ASD is flag", dictionary, localContext));
            Assert.AreEqual("ASD", localContext.Args["name"]);
            Assert.AreEqual(true, TryParseText("GDB is flag", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.Args["name"]);
            
            Assert.AreEqual(true, TryParseText("GDB is ADB", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.Args["name"]);
            Assert.AreEqual("ADB", localContext.Args["component"]);
            
            Assert.AreEqual(true, TryParseText("first second is struct", dictionary, localContext));
            Assert.AreEqual("second", localContext.Args["name"]);
            Assert.AreEqual("first", localContext.Args["treeName"]);

            Assert.AreEqual(true,
                TryParseText("ASD is flag , c is component : gg is struct", dictionary, localContext));
            Assert.AreEqual(false,
                TryParseText("ASD is flag , c is component : gg is struct asddd", dictionary, localContext));
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
            compiler.Dictionary.Load(Resources.Load<TextAsset>("TestDictionary").text);
            var source = Resources.Load<TextAsset>("TestDslProgram").text;
            var ctx = new CompilerContext()
            {
                Namespace = "Test"
            };
            ctx.Usings.Add("System.Collections");
            compiler.Build(source, ctx);
            Assert.AreEqual(0, ctx.UnparsedSentences.Count);
            Debug.LogWarning(ctx);
        }
        
        [Test]
        public void TestGameProgram()
        {
            var compiler = new DslCompiler();
            compiler.Dictionary.Load(Resources.Load<TextAsset>("DslDictionary").text);
            var source = File.ReadAllText("Assets/GDD.md");
            var ctx = new CompilerContext()
            {
                Namespace = "Test"
            };
            ctx.Usings.Add("System.Collections");
            compiler.Build(source, ctx);
            Assert.AreEqual(0, ctx.UnparsedSentences.Count);
            Debug.LogWarning(ctx);
        }

        [Test]
        public void TestMdParsing()
        {
            var compiler = new DslCompiler();
            var astConstructor = compiler.ProgramParser;
            Assert.IsNotNull(astConstructor);
            
            using var filestream = File.OpenRead("Assets/GDD.md");
            var ast = astConstructor.Parse(filestream);
            Assert.IsNotNull(ast);

            Debug.LogWarning(ast);
        }
    }
}