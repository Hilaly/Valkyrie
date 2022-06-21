using System.IO;
using System.Text;
using UnityEngine;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Language.Compiler
{
    static class LanguageAstConstructor
    {
        private static IAstConstructor _astConstructor;

        internal static Stream ToStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

        internal static IAstConstructor Constructor
        {
            get
            {
                if (_astConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("LogicLangGrammar").text;
                    using var dataStream = ToStream(data);
                    _astConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _astConstructor;
            }
        }

        public static IAstNode Parse(string txtProgram)
        {
            using var stream = txtProgram.ToStream();
            var ast = LanguageAstConstructor.Constructor.Parse(stream);
            return ast;
        }
    }
}