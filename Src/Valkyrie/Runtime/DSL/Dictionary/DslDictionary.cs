using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSL.Actions;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    class DslDictionary : IDslDictionary
    {
        private readonly List<DslDictionaryEntry> _entries = new List<DslDictionaryEntry>();

        public IEnumerable<IDslDictionaryEntry> GetEntries => _entries;
        
        public void Load(string text)
        {
            var ast = AstProvider.DictionaryConstructor.Parse(text.ToStream());
            Parse(ast);
        }

        private void Parse(IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<root>":
                case "<rules>":
                    foreach (var astNode in children) 
                        Parse(astNode);
                    break;
                case "<rule>":
                    var syntaxNode = children[0];
                    var actionNodes = children[2].UnpackNodes(x => x.Name == "<rule_action>");

                    var syntax = ParseRuleSyntax(syntaxNode);
                    if (syntax.Count(x => x is OperatorFormatEntry) != 1)
                        throw new GrammarCompileException(ast, "Only support one operator per rule");
                    
                    var actions = actionNodes.Select(x => ParseRuleAction(x, syntax)).ToList();
                    
                    _entries.Add(new DslDictionaryEntry()
                    {
                        Format = syntax,
                        Actions = actions
                    });
                    break;
                default:
                    throw new GrammarCompileException(ast, "Not implemented node type");
            }
        }

        private IDslAction ParseRuleAction(IAstNode astNode, List<DslDictionaryFormatEntry> syntax)
        {
            var name = astNode.Name;
            var children = astNode.GetChildren();
            switch (astNode.Name)
            {
                case "<rule_action>":
                    return ParseRuleAction(children[0], syntax);
                case "<skip_action>":
                    return new SkipAction();
                case "<create_type_action>":
                    return new CreateTypeAction() { Name = children[2].GetString(), Type = children[1].GetString() };
                default:
                    throw new GrammarCompileException(astNode, $"Unknown action node {name}");
            }
        }

        private List<DslDictionaryFormatEntry> ParseRuleSyntax(IAstNode ast)
        {
            var args = ast.UnpackNodes(x => x.Name == "<rule_arg>");
            return args.Select(x =>
            {
                var innerNode = x.GetChildren()[0];
                switch (innerNode.Name)
                {
                    case "<rule_var>":
                        return (DslDictionaryFormatEntry)new IdentifierFormatEntry(ASString(innerNode));
                    case "<rule_op>":
                        return (DslDictionaryFormatEntry)new OperatorFormatEntry(ASString(innerNode));
                    default:
                        throw new GrammarCompileException(innerNode, "Unsupported dictionary node");
                }
            }).ToList();
        }

        string ASString(IAstNode node)
        {
            switch (node.Name)
            {
                case "<rule_var>":
                    return node.GetString();
                case "<rule_op>":
                    return $"<{node.UnpackNodes(x => x.Name == "<id>").Select(x => x.GetString()).Join(" ")}>";
                default:
                    throw new GrammarCompileException(node, "Unimplemented ToString method");
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var entry in _entries) 
                sb.AppendLine(entry.ToString());
            return sb.ToString();
        }
    }
}