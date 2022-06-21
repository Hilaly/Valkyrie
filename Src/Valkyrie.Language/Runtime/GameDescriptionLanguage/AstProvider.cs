using System.IO;
using System.Text;
using UnityEngine;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    static class AstProvider
    {
        private static IAstConstructor _astConstructor;
        private static IAstConstructor _logicAstConstructor;

        internal static IAstConstructor Constructor
        {
            get
            {
                if (_astConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("DescriptionLanguageGrammar").text;
                    using var dataStream = ToStream(data);
                    _astConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _astConstructor;
            }
        }

        internal static IAstConstructor LogicConstructor
        {
            get
            {
                if (_logicAstConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("LogicLangGrammar").text;
                    using var dataStream = ToStream(data);
                    _logicAstConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _logicAstConstructor;
            }
        }

        public static IAstNode Parse(this IAstConstructor astConstructor, string txtProgram)
        {
            using var stream = txtProgram.ToStream();
            var ast = astConstructor.Parse(stream);
            return ast;
        }
        
        private static Stream ToStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }

    }
}