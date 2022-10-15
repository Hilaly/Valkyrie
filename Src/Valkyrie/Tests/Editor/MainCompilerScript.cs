using System.Collections;
using Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Valkyrie.Language.Description;

namespace Valkyrie.Language
{
    public class MainCompilerScript
    {
        [Test]
        public void TestDescCreation()
        {
            Assert.NotNull(Compiler.TestWorldDescCreation());
        }

        [Test]
        public void TestLogicCreation()
        {
            Assert.NotNull(Compiler.TestWorldLogicCreation());
        }

        [Test]
        public void TestLogicProgram()
        {
            var asset = Resources.Load<TextAsset>("TestLogicProgram").text;
            var worldDescription = new WorldDescription();
            Compiler.CompileWorldLogic(worldDescription, asset);
            Debug.Log(worldDescription);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MainCompilerScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public void GenerateProjectFiles()
        {
            ValkyrieGameLanguageCompiler.Compile();
        }
    }
}
