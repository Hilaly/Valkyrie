using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    static class AstProvider
    {
        private static IAstConstructor _dictionaryConstructor;
        private static IAstConstructor _programConstructor;

        public static IAstConstructor DictionaryConstructor
        {
            get
            {
                if (_dictionaryConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("DslDictionaryGrammar").text;
                    using var dataStream = data.ToStream();
                    _dictionaryConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _dictionaryConstructor;
            }
        }

        public static IAstConstructor ProgramConstructor
        {
            get
            {
                if (_programConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("DslProgramGrammar").text;
                    using var dataStream = data.ToStream();
                    _programConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _programConstructor;
            }
        }
    }
}