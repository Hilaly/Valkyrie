using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Valkyrie.Grammar
{
    public static class Grammar
    {
        enum ReadMode
        {
            Unknown,
            Lexer,
            Parser,
            Optimizer
        }

        public static IGrammarDefinition Empty => new GrammarDefinition();

        public static IGrammarDefinition Parse(Stream grammarDefinitionStream)
        {
            var result = new GrammarDefinition();
            var mode = ReadMode.Unknown;
            using (var reader = new StreamReader(grammarDefinitionStream))
            {
                int lineNumber = -1;
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    ++lineNumber;

                    if (string.IsNullOrEmpty(line))
                        continue;

                    //Parse parameters
                    if (line.StartsWith("#"))
                    {
                        var parameters = line.Substring(1)
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var parameter in parameters)
                        {
                            var skipParameter = false;
                            var breakParse = false;

                            switch (parameter)
                            {
                                case "lexer":
                                    mode = ReadMode.Lexer;
                                    break;
                                case "parser":
                                    mode = ReadMode.Parser;
                                    break;
                                case "optimizer":
                                    mode = ReadMode.Optimizer;
                                    break;
                                case "comment":
                                    skipParameter = true;
                                    breakParse = true;
                                    break;
                            }

                            if (!skipParameter)
                                result.Parameters.Add(parameter);
                            if (breakParse)
                                break;
                        }
                    }
                    //Parse nodes
                    else
                    {
                        switch (mode)
                        {
                            case ReadMode.Unknown:
                                break;
                            case ReadMode.Lexer:
                            {
                                ReadLexerLine(line, result, lineNumber);
                                break;
                            }
                            case ReadMode.Parser:
                            {
                                ReadParserLine(line, result, lineNumber);
                                break;
                            }
                            case ReadMode.Optimizer:
                            {
                                ReadOptimizerLine(line, result, lineNumber);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }

            return result;
        }

        #region Read impl

        private static void ReadOptimizerLine(string line, GrammarDefinition result, int lineNumber)
        {
        }

        private static void ReadParserLine(string line, GrammarDefinition result, int lineNumber)
        {
            var count = line.Split(new[] { "->", "::=" }, StringSplitOptions.None);
            if (count.Length < 2)
            {
                count = line.Split(new[] { "<-" }, StringSplitOptions.None);
                if (count.Length < 2)
                    throw new GrammarParseException(line, lineNumber, 0, $"Parts of grammar must be splitted by ::=");
                else
                {
                    var nodeName = GetNodeName(count[0]);
                    var node = result.Nodes.Find(u => u.Name == nodeName);
                    if (node == null)
                        result.Nodes.Add(node = new GrammarNodeDefinition { Name = nodeName });
                    var args = count[1].Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                    var parentNode = result.Nodes.Find(u => u.Name == args[0]);
                    var rems = args[1].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var nodeVariant in parentNode.Variants)
                    {
                        var nvStr = string.Join(" ", nodeVariant);
                        if (rems.Contains(nvStr))
                            continue;
                        var def = nvStr;
                    }
                }
            }
            else
            {
                var nodeName = GetNodeName(count[0]);
                var node = result.Nodes.Find(u => u.Name == nodeName);
                if (node == null)
                    result.Nodes.Add(node = new GrammarNodeDefinition { Name = nodeName });

                var defs = count[1].Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var def in defs)
                {
                    var source = def.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (source.Length == 0)
                        continue;

                    var variants = new List<List<string>> { new List<string>() };

                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var i = 0; i < source.Length; ++i)
                    {
                        var currentText = source[i];
                        if (IsZeroOrOne(currentText, out var nodename))
                            Clone(variants, 1, (index, list) =>
                            {
                                if (index == 0)
                                    list.Add(nodename);
                            });
                        else if (IsZeroOrAny(currentText, out nodename))
                        {
                            var nodeListName = GenerateNodeList(nodename, result, true);
                            ForAll(variants, (list) => list.Add(nodeListName));
                        }
                        else if (IsOneOrAny(currentText, out nodename))
                        {
                            var nodeListName = GenerateNodeList(nodename, result, false);
                            ForAll(variants, (list) => list.Add(nodeListName));
                        }
                        else
                        {
                            nodename = GetNodeValue(currentText);
                            if (!string.IsNullOrEmpty(nodename))
                                ForAll(variants, (list) => list.Add(nodename));
                        }
                    }

                    node.Variants.AddRange(variants);
                }
            }
        }

        private static void ReadLexerLine(string line, GrammarDefinition result, int lineNumber)
        {
            var hasName = line.LastIndexOf("->", StringComparison.InvariantCultureIgnoreCase);
            if (hasName < 0)
                result.Lexem.Add(new KeyValuePair<Regex, string>(new Regex(line.Trim()), null));
            else
                result.Lexem.Add(new KeyValuePair<Regex, string>(
                    new Regex(line.Substring(0, hasName).Trim()),
                    line.Substring(hasName + "->".Length).Trim()));
        }

        #endregion

        #region Utils

        private static string GenerateNodeList(string baseNodeName, GrammarDefinition result, bool allowZeroCount)
        {
            var nodeListName = GetNodeListName(baseNodeName, allowZeroCount);
            var listNode = result.Nodes.Find(u => u.Name == nodeListName);
            if (listNode == null)
            {
                result.Nodes.Add(listNode = new GrammarNodeDefinition { Name = nodeListName });
                listNode.Variants.Add(new List<string>(new[]
                {
                    baseNodeName,
                    nodeListName
                }));
                listNode.Variants.Add(new List<string>(new[]
                {
                    baseNodeName
                }));
                if (allowZeroCount)
                    listNode.Variants.Add(new List<string>());
            }

            return nodeListName;
        }

        private static void Clone(List<List<string>> variants, int cloneCount, Action<int, List<string>> actionForClone)
        {
            var allNew = new List<List<string>>();
            for (var i = 0; i < cloneCount; ++i)
            {
                var temp = variants.Select(u => new List<string>(u)).ToList();
                allNew.AddRange(temp);
                foreach (var list in temp)
                    actionForClone(i + 1, list);
            }

            foreach (var list in variants)
                actionForClone(0, list);

            variants.AddRange(allNew);
        }

        private static void ForAll(List<List<string>> variants, Action<List<string>> actionForList)
        {
            variants.ForEach(actionForList);
        }

        internal static string GetNodeListName(string s, bool allowZero)
        {
            return $"<generated-{(allowZero ? "zerocount" : "")}list-{s}>";
        }

        private static string GetNodeValue(string s)
        {
            var result = Regex.Match(s, "^\"(?<nodename>.*)\"$");
            return result.Success
                ? result.Groups["nodename"].Value
                : s;
        }

        static string GetNodeName(string s)
        {
            return s.Trim().Replace(" ", "");
        }

        static bool IsZeroOrOne(string s, out string extractedNode)
        {
            var result = Regex.Match(s, @"^\[(?<nodename>.+)\]$");
            extractedNode = result.Success ? result.Groups["nodename"].Value : null;
            return result.Success;
        }

        static bool IsZeroOrAny(string s, out string extractedNode)
        {
            var result = Regex.Match(s, @"^(?<nodename>.+)\*$");
            extractedNode = result.Success ? result.Groups["nodename"].Value : null;
            return result.Success;
        }

        static bool IsOneOrAny(string s, out string extractedNode)
        {
            var result = Regex.Match(s, @"^(?<nodename>.+)\+$");
            extractedNode = result.Success ? result.Groups["nodename"].Value : null;
            return result.Success;
        }

        #endregion

        public static ILexer CreateLexer(IGrammarDefinition grammarDefinition)
        {
            var c = (GrammarDefinition)grammarDefinition;
            if (c.Lexer)
            {
                return new RegexLexer(c.Lexem, c.EscapeLexem);
            }

            return new Lexer(c.ReadEscape, c.ReadEol);
        }

        public static IAstConstructor Create(Stream grammarDefinitionStream)
        {
            var grammarDefinition = Parse(grammarDefinitionStream);
            return new GrammarParser((GrammarDefinition)grammarDefinition);
        }

        public static float GetFloat(this IAstNode node)
        {
            return float.Parse(node.GetString(), CultureInfo.InvariantCulture);
        }

        public static bool GetBool(this IAstNode node)
        {
            return bool.Parse(node.GetString());
        }

        static readonly Regex GeneratedUnpackRegex = new Regex("<generated-(zerocount)?list-(?<name><\\w*>)>");
        
        public static List<IAstNode> UnpackGeneratedLists(this IAstNode node)
        {
            var regex = GeneratedUnpackRegex;
            var result = new List<IAstNode>();
            var children = node.GetChildren(false);
            foreach (var t in children)
            {
                var m = regex.Match(t.Name);
                if (m.Success)
                {
                    var id = m.Groups["name"].Value;
                    result.AddRange(t.UnpackNodes(x => x.Name == id));
                }
                else
                    result.Add(t);
            }

            //UnityEngien.Debug.LogWarning($"{children.Select(x => x.Name).Join(",")} became {result.Select(x => x.Name).Join(",")}");
            return result;
        }

        public static List<IAstNode> UnpackNodes(this IAstNode node, Func<IAstNode, bool> filter)
        {
            var r = new List<IAstNode>();
            if (filter(node))
                r.Add(node);
            foreach (var child in node.GetChildren())
                r.AddRange(UnpackNodes(child, filter));
            return r;
        }

        public static int GetInt(this IAstNode node)
        {
            return int.Parse(node.GetString(), CultureInfo.InvariantCulture);
        }

        public static string ConvertTreeToString(this IAstNode node, string del = " ")
        {
            var sList = new List<string>();
            var enumerator = node.EnumerateTerminalNodes();
            while (enumerator.MoveNext())
                sList.Add(enumerator.Current.GetString());
            return string.Join(del, sList);
        }

        public static string GetString(this IAstNode node)
        {
            if (node is TerminalNode terminalNode)
                return terminalNode.Lexem.Value;
            var n = (NonTerminalNode)node;
            if (n.Nodes.Count != 1)
                throw new Exception($"{node.Name} is not simple");
            return n.Nodes[0].GetString();
        }

        internal static List<IAstNode> FindAllInList(this IAstNode node, string listName, string nodeName)
        {
            var result = new List<IAstNode>();
            foreach (var astNode in node.GetChildren())
            {
                if (astNode.Name == listName)
                    result.AddRange(FindAllInList(astNode, listName, nodeName));
                else if (astNode.Name == nodeName)
                    result.Add(astNode);
            }

            return result;
        }


        internal static IAstNode Find(this IAstNode node, string id)
        {
            var list = node.FindAll(id);
            return list.Count > 0
                ? list[0]
                : null;
        }

        internal static IEnumerable<IAstNode> Iterate(this IAstNode node)
        {
            yield return node;
            if (node is NonTerminalNode nt)
                foreach (var astNode in nt.Nodes)
                foreach (var child in astNode.Iterate())
                    yield return child;
        }

        internal static List<IAstNode> FindAll(this IAstNode node, string id)
        {
            var parts = id.Split('.').ToList();
            var result = new List<IAstNode>();
            if (parts.Count == 1)
            {
                if (node is NonTerminalNode nt)
                {
                    var listNames = new[]
                    {
                        GetNodeListName(id, true),
                        GetNodeListName(id, false)
                    };
                    foreach (var ntNode in nt.Nodes)
                    {
                        if (ntNode.Name == id)
                            result.Add(ntNode);
                        else if (listNames.Contains(ntNode.Name))
                            result.AddRange(ntNode.FindAll(id));
                    }
                }
            }
            else
            {
                var nid = string.Join(".", parts.GetRange(1, parts.Count - 1));
                result.AddRange(node.FindAll(parts[0]).SelectMany(u => u.FindAll(nid)));
            }

            return result;
        }
    }
}