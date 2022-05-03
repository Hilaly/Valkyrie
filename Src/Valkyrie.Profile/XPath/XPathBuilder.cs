using System.IO;
using System.Text;
using UnityEngine;
using Valkyrie.Grammar;

namespace Valkyrie.XPath
{
    static class XPathBuilder
    {
        private static readonly XPathCompiler PathCompiler = new XPathCompiler();
        
        private static IAstConstructor _astConstructor;

        static IAstConstructor Constructor
        {
            get
            {
                if (_astConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("XPathGrammar").text;
                    using var dataStream = ToStream(data);
                    _astConstructor = Grammar.Grammar.Create(dataStream);
                }

                return _astConstructor;
            }
        }

        public static XPath Build(string xPathExpression)
        {
            return new XPath(xPathExpression);
        }

        internal static IPathSelector BuildSelector(string expression)
        {
            using var stream = ToStream(expression);
            var ast = Constructor.Parse(stream);
            var selector = PathCompiler.Compile(ast);
            return selector;
        }
        
        static Stream ToStream(this string source)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(source));
        }
    }
}