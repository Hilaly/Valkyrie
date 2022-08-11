using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valkyrie.DSL.Actions;
using Valkyrie.DSL.Expressions;
using Valkyrie.DSL.StringWorking;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Dictionary
{
    class DslDictionary : IDslDictionary
    {
        private readonly List<DslDictionaryNode> _nodes = new();
        private readonly List<DslMacro> _macros = new();

        internal DslDictionaryNode Get(string name, bool create = false)
        {
            var r = _nodes.Find(x => x.Name == name);
            if (create && r == null)
                _nodes.Add(r = new DslDictionaryNode(name));
            return r;
        }

        public IEnumerable<IDslDictionaryEntry> GetEntries => _nodes.SelectMany(x => x.GetEntries);
        public IEnumerable<IDslMacro> GetMacros => _macros;
        
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
                case "<some_entry>":
                    foreach (var astNode in children)
                        Parse(astNode);
                    break;
                case "<macro>":
                    var macroSource = children.Find(x => x.Name == "<macro_source>");
                    var macroSentence = children.Find(x => x.Name == "<macro_sentence>");
                    CreateMacro(macroSource, macroSentence);
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

        private void CreateMacro(IAstNode macroSource, IAstNode macroSentence)
        {
            _macros.Add(new DslMacro(macroSource.GetString().Trim('"'), macroSentence.GetString().Trim('"')));
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
            if (astNode == null)
                return new SkipAction();
            
            var name = astNode.Name;
            var children = astNode.UnpackGeneratedLists();
            switch (astNode.Name)
            {
                case "<rule_actions>":
                {
                    var actions = astNode
                        .UnpackNodes(x => x.Name == "<rule_action>")
                        .Select(x => ParseRuleAction(x, syntax))
                        .ToList();
                    return new SequenceAction(actions);
                }
                case "<rule_action>":
                    if(children.Count == 1)
                        return ParseRuleAction(children[0], syntax);
                    if (children.Count == 2)
                        return new SkipAction();
                    return ParseRuleAction(children[1], syntax);
                case "<true_if_branch>":
                case "<false_if_branch>":
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
                    return new AddBaseTypeAction()
                    {
                        Type = CreateStringProvider(children[4], syntax),
                        BaseType = CreateStringProvider(children[2], syntax)
                    };
                case "<add_attribute_action>":
                    return new AddAttributeToTypeAction()
                    {
                        Type = CreateStringProvider(children[4], syntax),
                        Attribute = CreateStringProvider(children[2], syntax)
                    };
                case "<push_local_up_action>":
                    return new PushLocalUpAction()
                    {
                        LocalVarName = CreateStringProvider(children[2], syntax)
                    };
                case "<add_method_action>":
                    return new AddMethodToTypeAction()
                    {
                        Type = CreateStringProvider(children[4], syntax),
                        Method = CreateStringProvider(children[2], syntax)
                    };
                case "<log_action>":
                    return new LogAction()
                    {
                        Text = CreateStringProvider(children[1], syntax)
                    };
                case "<call_action>":
                    return new CallAction()
                    {
                        ChildName = CreateStringProvider(children[1], syntax)
                    };
                case "<set_global_action>":
                    return new SetGlobalVarAction
                    {
                        Value = CreateStringProvider(children[4], syntax),
                        Name = CreateStringProvider(children[2], syntax)
                    };
                case "<set_local_action>":
                    return new SetLocalVarAction
                    {
                        Value = CreateStringProvider(children[4], syntax),
                        Name = CreateStringProvider(children[2], syntax)
                    };
                case "<if_action>":
                    return new IfAction()
                    {
                        Expr = ParseRuleExpr(children.Find(x => x.Name == "<expr>"), syntax),
                        TrueAction = ParseRuleAction(children.Find(x => x.Name == "<true_if_branch>"), syntax),
                        FalseAction = ParseRuleAction(children.Find(x => x.Name == "<false_if_branch>"), syntax)
                    };
                case "<add_property_action>":
                {
                    return new AddPropertyToTypeAction()
                    {
                        Type = CreateStringProvider(children[4], syntax),
                        Property = CreateStringProvider(children[2], syntax)
                    };
                }
                case "<add_code_to_property_getter>":
                {
                    return new AddCodeToGetterAction()
                    {
                        Type = CreateStringProvider(children[7], syntax),
                        Property = CreateStringProvider(children[5], syntax),
                        Code = CreateStringProvider(children[2], syntax)
                    };
                }
                case "<add_code_to_property_setter>":
                {
                    return new AddCodeToSetterAction()
                    {
                        Type = CreateStringProvider(children[7], syntax),
                        Property = CreateStringProvider(children[5], syntax),
                        Code = CreateStringProvider(children[2], syntax)
                    };
                }
                case "<set_property_type>":
                {
                    return new SetPropertyTypeAction()
                    {
                        Type = CreateStringProvider(children[5], syntax),
                        Property = CreateStringProvider(children[3], syntax),
                        Code = CreateStringProvider(children[7], syntax)
                    };
                }
                case "<set_method_return_type>":
                {
                    return new SetMethodTypeAction()
                    {
                        Type = CreateStringProvider(children[5], syntax),
                        Method = CreateStringProvider(children[3], syntax),
                        Code = CreateStringProvider(children[7], syntax)
                    };
                }
                case "<add_code_global>":
                {
                    return new AddCodeGlobalAction()
                    {
                        Code = CreateStringProvider(children[2], syntax)
                    };
                }
                default:
                    throw new GrammarCompileException(astNode, $"Unknown action node {name}");
            }
        }

        private IDslExpr ParseRuleExpr(IAstNode astNode, List<DslDictionaryFormatEntry> syntax)
        {
            var name = astNode.Name;
            var children = astNode.UnpackGeneratedLists();
            switch (name)
            {
                case "<or_expr>":
                    if (children.Count == 1)
                        return ParseRuleExpr(children[0], syntax);
                    return new OrExpr()
                    {
                        Left = ParseRuleExpr(children[0], syntax),
                        Right = ParseRuleExpr(children[2], syntax)
                    };
                case "<and_expr>":
                    if (children.Count == 1)
                        return ParseRuleExpr(children[0], syntax);
                    return new AndExpr()
                    {
                        Left = ParseRuleExpr(children[0], syntax),
                        Right = ParseRuleExpr(children[2], syntax)
                    };
                case "<single_expr>":
                    if(children.Count == 1)
                        return ParseRuleExpr(children[0], syntax);
                    return ParseRuleExpr(children[1], syntax);
                case "<not_expr>":
                    if (children.Count == 1)
                        return ParseRuleExpr(children[0], syntax);
                    return new NotExpr()
                    {
                        Expr = ParseRuleExpr(children[1], syntax)
                    };
                case "<global_expr>":
                    return new GlobalExpr()
                    {
                        Name = CreateStringProvider(children[1], syntax)
                    };
                case "<local_expr>":
                    return new LocalExpr()
                    {
                        Name = CreateStringProvider(children[1], syntax)
                    };
                case "<expr>":
                    return ParseRuleExpr(children[0], syntax);
                default:
                    throw new GrammarCompileException(astNode, $"Unknown EXPR node {name}");
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
                                         x is LocalVariableEntry idFormat && idFormat.Text == value)
                                 != null;
                    if (hasVar)
                        return new LocalVariableStringProvider(value);
                    else
                        return new ConstantStringProvider(value);
                }
                case "<rule_var>":
                {
                    var value = ast.UnpackGeneratedLists()[1].GetString();
                    var hasVar = syntax
                                     .Find(x =>
                                         x is LocalVariableEntry idFormat && idFormat.Text == value)
                                 != null;
                    if (hasVar)
                        return new LocalVariableStringProvider(value);
                    throw new GrammarCompileException(ast, $"Unknown local var {value}");
                }
                case "<global_var>":
                {
                    var value = ast.UnpackGeneratedLists()[1].GetString();
                    return new GlobalVariableStringProvider(value);
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
                                             x is LocalVariableEntry idFormat && idFormat.Text == value)
                                     != null;
                        if (hasVar)
                            return new LocalVariableStringProvider(value);
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
                    yield return new LocalVariableEntry(text);
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