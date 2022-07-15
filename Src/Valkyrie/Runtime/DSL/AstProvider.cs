using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    static class AstProvider
    {
        private static IAstConstructor _astConstructor;

        public static IAstConstructor DictionaryConstructor
        {
            get
            {
                if (_astConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("DslDictionaryGrammar").text;
                    using var dataStream = data.ToStream();
                    _astConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _astConstructor;
            }
        }
    }
}