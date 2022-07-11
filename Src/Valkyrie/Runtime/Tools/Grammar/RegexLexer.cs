using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Valkyrie.Grammar
{
    class RegexLexer : ILexer
    {
        readonly List<KeyValuePair<Regex, string>> _matchers;
        private readonly Regex _escape;

        public override string ToString()
        {
            var sb = new StringBuilder("Lexer: ");
            foreach (var matcher in _matchers) sb.Append(" ").Append(matcher.Key);
            sb.Append(_escape);
            return sb.ToString();
        }

        public RegexLexer(List<KeyValuePair<Regex, string>> matchers, Regex escape)
        {
            _matchers = matchers;
            _escape = escape;
        }

        public List<Lexem> Parse(Stream stream)
        {
            var result = new List<Lexem>();
            var source = new StreamReader(stream).ReadToEnd();
            var index = 0;
            while (index < source.Length)
            {
                var founded = false;
                foreach (var matcher in _matchers)
                {
                    var matchResult = matcher.Key.Match(source, index);
                    if(!matchResult.Success)
                        continue;
                    if(matchResult.Index > index)
                        continue;
                    index += matchResult.Length;
                    
                    //Skip escape and comments
                    if (matcher.Key != _escape && matcher.Value != "comment")
                        result.Add(new Lexem {Name = matcher.Value ?? matchResult.Value, Value = matchResult.Value});
                    
                    founded = true;
                    break;
                }

                if (!founded)
                {
                    var lineNum = 1;
                    var lastIndex = 0;
                    for (var i = 0; i < index; ++i)
                    {
                        if (source[i] != '\n')
                            continue;
                        
                        lineNum += 1;
                        lastIndex = i;
                    }
                    var res = _escape.Match(source, index);
                    var text = res.Success
                        ? source.Substring(index, res.Index - index)
                        : source.Substring(index);
                    throw new Exception(
                        $"Can not parse [{(text.Length > 10 ? text.Substring(0, 10) : text)}] at line={lineNum} column={index - lastIndex}");
                }
            }

            return result;
        }
    }
}