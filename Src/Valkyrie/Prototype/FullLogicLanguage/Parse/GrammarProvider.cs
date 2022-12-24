using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie
{
    public class GrammarProvider
    {
        private IAstConstructor _programConstructor;

        public IAstConstructor ProgramConstructor
        {
            get
            {
                if (_programConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("FullLogicLangGrammar").text;
                    using var dataStream = data.ToStream();
                    _programConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _programConstructor;
            }
        }
    }
}