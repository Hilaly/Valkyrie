using System;

namespace Valkyrie.Grammar
{
    public class GrammarParseException : Exception
    {
        public string Text { get; }
        public int Line { get; }
        public int Column { get; }
        public string Description { get; }

        public GrammarParseException(string text, int line, int column, string s = null) : base($"{s ?? "Exception during parse"} at '{text}':line {line}, column {column}")
        {
            Text = text;
            Line = line;
            Column = column;
            Description = s;
        }
    }
}