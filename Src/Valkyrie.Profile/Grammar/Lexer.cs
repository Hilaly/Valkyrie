using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Valkyrie.Grammar
{
    class Lexer : ILexer
    {
        private readonly bool _readEscape;
        private readonly bool _readEol;
        private readonly Regex _number = new Regex("(-|[0-9])");
        private readonly Regex _char = new Regex("[A-Za-z]");
        private readonly Regex _esc = new Regex("[ \t\r\n]");

        public Lexer(bool readEscape, bool readEol)
        {
            _readEscape = readEscape;
            _readEol = readEol;
        }

        public List<Lexem> Parse(Stream stream)
        {
            var reader = new StreamReader(stream);
            var result = new List<Lexem>();

            string current = string.Empty;

            while (!reader.EndOfStream)
            {
                var value = (char)reader.Read();
                var test = new string(value, 1);
                if (_esc.IsMatch(test))
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        result.Add(new Lexem { Name = Categorize(current), Value = current });
                        current = string.Empty;
                    }

                    if (test == "\n" && _readEol || _readEscape)
                        result.Add(new Lexem { Name = Categorize(test), Value = test });

                    continue;
                }

                if (_char.IsMatch(test))
                {
                    current += test;
                    continue;
                }

                if (_number.IsMatch(test) || value == '.')
                {
                    current += test;
                    continue;
                }

                switch (value)
                {
                    case '(':
                    case ')':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case ',':
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                    case '=':
                    case ';':
                    case '<':
                    case '>':
                    case '!':
                    case ':':
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            result.Add(Create(current));
                            current = string.Empty;
                        }

                        result.Add(Create(test));
                        continue;
                    }
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                result.Add(Create(current));
                current = string.Empty;
            }

            return result;
        }

        Lexem Create(string value)
        {
            return new Lexem() { Name = Categorize(value), Value = value };
        }

        private string Categorize(string value)
        {
            return value;
        }
    }
}