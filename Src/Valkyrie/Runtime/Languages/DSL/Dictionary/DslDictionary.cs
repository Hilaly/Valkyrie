using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Graphs;
using UnityEngine;
using Valkyrie.DSL.Actions;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Dictionary
{
    class DslDictionaryNode
    {
        public string Name { get; }

        private readonly List<DslDictionaryEntry> _entries = new();

        public IEnumerable<IDslDictionaryEntry> GetEntries => _entries;

        public DslDictionaryNode(string name) => Name = name;

        public void Add(DslDictionaryEntry dslDictionaryEntry) => _entries.Add(dslDictionaryEntry);
    }

    class DslDictionary : IDslDictionary
    {
        private readonly List<DslDictionaryNode> _nodes = new();

        private DslDictionaryNode Get(string name, bool create = false)
        {
            var r = _nodes.Find(x => x.Name == name);
            if (create && r == null)
                _nodes.Add(r = new DslDictionaryNode(name));
            return r;
        }

        public IEnumerable<IDslDictionaryEntry> GetEntries => _nodes.SelectMany(x => x.GetEntries);

        public void Load(string text)
        {
            var ast = AstProvider.DictionaryConstructor.Parse(text.ToStream());

            Parse(ast);
        }

        private void Parse(IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<root>":
                    foreach (var astNode in children)
                        Parse(astNode);
                    break;
                case "<rule>":
                    var rn = children.Find(x => x.Name == "<rule_name>");
                    var bnf = children.Find(x => x.Name == "<rule_bnf>");
                    var ra = children.Find(x => x.Name == "<rule_actions>");

                    ParseRule(rn, bnf, ra);
                    break;
                default:
                    throw new GrammarCompileException(ast, "Not implemented node type");
            }
        }

        private void ParseRule(IAstNode rn, IAstNode bnf, IAstNode ra)
        {
            var node = Get(rn.GetString(), true);

            var syntax = ParseRuleSyntax(bnf);

            var actions = ra
                .UnpackNodes(x => x.Name == "<rule_action>")
                .Select(x => ParseRuleAction(x, syntax))
                .ToList();

            node.Add(new DslDictionaryEntry()
            {
                Format = syntax,
                Actions = actions
            });
        }

        private IDslAction ParseRuleAction(IAstNode astNode, List<DslDictionaryFormatEntry> syntax)
        {
            var name = astNode.Name;
            var children = astNode.UnpackGeneratedLists();
            switch (astNode.Name)
            {
                case "<rule_action>":
                    return ParseRuleAction(children[0], syntax);
                case "<skip_action>":
                    return new SkipAction();
                case "<create_type_action>":
                    return new CreateTypeAction()
                    {
                        Type = CreateStringProvider(children[1], syntax),
                        Name = CreateStringProvider(children[2], syntax)
                    };
                case "<add_base_type_action>":
                    return new AddbaseTypeAction()
                    {
                        Type = CreateStringProvider(children[4], syntax),
                        BaseType = CreateStringProvider(children[2], syntax)
                    };
                default:
                    throw new GrammarCompileException(astNode, $"Unknown action node {name}");
            }
        }

        internal static IStringProvider CreateStringProvider(IAstNode ast, List<DslDictionaryFormatEntry> syntax)
        {
            switch (ast.Name)
            {
                case "<final_str_literal>":
                case "<word>":
                    return CreateStringProvider(ast.UnpackGeneratedLists()[0], syntax);
                case "<add_words_op>":
                {
                    var children = ast.UnpackGeneratedLists();
                    if (children.Count == 3)
                        return new ConcatenateStringProvider()
                        {
                            CreateStringProvider(children[0], syntax),
                            CreateStringProvider(children[2], syntax)
                        };
                    return CreateStringProvider(children[0], syntax);
                }
                case "<string>":
                case "STRING":
                    return new ConstantStringProvider(ast.GetString());
                case "<id>":
                case "IDENTIFIER":
                {
                    var value = ast.GetString();
                    var hasVar = syntax
                                     .Find(x =>
                                         x is IdentifierFormatEntry idFormat && idFormat.Text == value)
                                 != null;
                    if (hasVar)
                        return new VariableStringProvider(value);
                    else
                        return new ConstantStringProvider(value);
                }
                default:
                    throw new GrammarCompileException(ast, $"Unimplemented STR LITERAL {ast.Name}");
            }

            while (true)
            {
                switch (ast.Name)
                {
                    case "<final_str_literal>":
                    case "<word>":
                        ast = ast.UnpackGeneratedLists()[0];
                        continue;
                    case "<add_words_op>":
                    {
                        var children = ast.UnpackGeneratedLists();
                        if (children.Count == 3)
                            return new ConcatenateStringProvider()
                            {
                                CreateStringProvider(children[0], syntax),
                                CreateStringProvider(children[2], syntax)
                            };
                        ast = children[0];
                        continue;
                    }
                    case "<string>":
                        return new ConstantStringProvider(ast.GetString());
                    case "<id>":
                    {
                        var value = ast.GetString();
                        var hasVar = syntax
                                         .Find(x =>
                                             x is IdentifierFormatEntry idFormat && idFormat.Text == value)
                                     != null;
                        if (hasVar)
                            return new VariableStringProvider(value);
                        else
                            return new ConstantStringProvider(value);
                    }
                    default:
                        throw new GrammarCompileException(ast, $"Unimplemented node {ast.Name}");
                }
            }
        }

        private List<DslDictionaryFormatEntry> ParseRuleSyntax(IAstNode ast) =>
            ast
                .UnpackNodes(x => x.Name == "<rule_bnf_entry>")
                .SelectMany(CreateFormatEntries)
                .ToList();

        IEnumerable<DslDictionaryFormatEntry> CreateFormatEntries(IAstNode node)
        {
            if (node.Name != "<rule_bnf_entry>")
                throw new GrammarCompileException(node, $"{node.Name} is not valid node for BNF");

            var innerNode = node.GetChildren(false)[0];

            switch (innerNode.Name)
            {
                case "<rule_op>":
                {
                    var text = innerNode.GetString().Trim('"');
                    var entries = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in entries)
                        yield return new OperatorFormatEntry(s);
                    yield break;
                }
                case "<rule_var>":
                {
                    var text = innerNode.GetChildren(false)[1].GetString();
                    yield return new IdentifierFormatEntry(text);
                    yield break;
                }
                case "<rule_bnf_match>":
                {
                    var text = innerNode.GetString();
                    yield return new ExtractTreeFormatEntry(text, this);
                    yield break;
                }
                default:
                    throw new GrammarCompileException(innerNode, $"{innerNode.Name} is not valid node for BNF");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in GetEntries)
                sb.AppendLine(entry.ToString());
            return sb.ToString();
        }
    }
}