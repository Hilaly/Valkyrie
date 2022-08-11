using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using Valkyrie.DSL;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

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
            Assert.AreEqual(0, localContext.GetLocalVariables().Count);

            localContext = new LocalContext();
            Assert.AreEqual(true, TryParseText("ASD is flag", dictionary, localContext));
            Assert.AreEqual("ASD", localContext.GetLocalVariables()["name"]);
            
            localContext = new LocalContext();
            Assert.AreEqual(true, TryParseText("GDB is flag", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.GetLocalVariables()["name"]);

            localContext = new LocalContext();
            Assert.AreEqual(true, TryParseText("GDB is ADB", dictionary, localContext));
            Assert.AreEqual("GDB", localContext.GetLocalVariables()["name"]);
            Assert.AreEqual("ADB", localContext.GetLocalVariables()["component"]);

            localContext = new LocalContext();
            Assert.AreEqual(true, TryParseText("first second is struct", dictionary, localContext));
            Assert.AreEqual(false, localContext.GetLocalVariables().ContainsKey("name"));
            Assert.AreEqual("first", localContext.GetLocalVariables()["treeName"]);

            localContext = new LocalContext();
            Assert.AreEqual(true,
                TryParseText("ASD is flag , c is component : gg is struct", dictionary, localContext));
            localContext = new LocalContext();
            Assert.AreEqual(false,
                TryParseText("ASD is flag , c is component : gg is struct asddd", dictionary, localContext));
        }

        bool TryParseText(string text, IDslDictionary dictionary, LocalContext localContext)
        {
            foreach (var entry in dictionary.GetEntries)
            {
                var t = new LocalContext();
                if (entry.TryMatch(text, t))
                {
                    localContext.ReplaceFrom(t);
                    return true;
                }
            }
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
            ctx.AddUsing("System.Collections");
            compiler.Build(source, ctx);
            Debug.Log(ctx);
            Assert.AreEqual(0, ctx.UnparsedSentences.Count);
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
            ctx.AddUsing("System.Collections");
            compiler.Build(source, ctx);
            Debug.Log(ctx);
            Assert.AreEqual(0, ctx.UnparsedSentences.Count);
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

            Debug.Log(ast);
        }

        [Test]
        public void TestCodeGenerating()
        {
            var context = new CompilerContext
            {
                Namespace = "Test"
            };
            context.AddUsing("System");
            var testClass = context.GetOrCreateType("TestClass");
            testClass.AddAttribute("CodeAttribute");
            testClass.AddBase("System.Object");
            testClass.AddBase("IComponent");
            var field = testClass.GetOrCreateField("_floatValue");
            field.Type = "float";
            field.Modificator = "protected";

            var autoProperty = testClass.GetOrCreateProperty("Auto");
            autoProperty.Modificator = "protected";
            autoProperty.Type = "int";

            var onlyGet = testClass.GetOrCreateProperty("Getter");
            onlyGet.Modificator = "public";
            onlyGet.Type = "int";
            onlyGet.GetGetter().AddCode("return Auto;");

            var onlySet = testClass.GetOrCreateProperty("Setter");
            onlySet.Modificator = "public";
            onlySet.Type = "int";
            onlySet.GetSetter().AddCode("Auto = value;");
            
            Debug.Log(context);
        }

        [Test]
        public void TestRegexReplace()
        {
            var source = "class Alpha{PROPERTY(int,M)PROPERTY(float,K)}";
            var regex = "PROPERTY\\((?'type'\\w+),(?'name'\\w+)\\)";
            var r = new Regex(regex);
            while (r.IsMatch(source))
            {
                source = r.Replace(source, "${type} ${name} {get;set;}");
            }

            Debug.Log(source);
        }
    }
}