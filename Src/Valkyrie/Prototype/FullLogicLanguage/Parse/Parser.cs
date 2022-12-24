using System.Linq;
using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie
{
    public class Parser
    {
        private readonly GrammarProvider _grammarProvider;

        public Parser(GrammarProvider grammarProvider)
        {
            _grammarProvider = grammarProvider;
        }

        public void Parse(GameDescription gameDescription, string text)
        {
            using var stream = text.ToStream();
            var ast = _grammarProvider.ProgramConstructor.Parse(stream);
            var ctx = new ParserContext { Game = gameDescription }.FillDefault();
            Parse(ctx, ast);
            Debug.LogWarning(ctx.Log());
        }

        private void Parse(ParserContext gameCtx, IAstNode ast)
        {
            var extracted = new ExtractedNode(ast)
                .On("root", node =>
                {
                    foreach (var astNode in node.Children)
                        Parse(gameCtx, astNode);
                })
                .On("full_sentence", node =>
                {
                    foreach (var astNode in node.FilterByName("sentence"))
                        Parse(gameCtx, astNode);
                })
                .On("sentence", node =>
                    Parse(gameCtx, node.Children.First()))
                .On("alias_sentence", node =>
                    ParseAlias(gameCtx, node))
                .On("component_sentence_define", node =>
                    ParseComponentDefine(gameCtx, node.ChildWithName("component_with_type")))
                .On("archetype_sentence_define", node =>
                    ParseArchetypeDefine(gameCtx, node))
                .On("requester_sentence_define", node => { })
                .Process();
        }

        #region Real parsing

        private void ParseArchetypeDefine(ParserContext gameCtx, ExtractedNode n)
        {
            var archetypeName = n.ChildWithName("archetype_name").GetStringWithoutQuote();
            var exist = gameCtx.Game.TryGetArchetype(archetypeName, true);

            gameCtx.CurrentArchetype = exist;
            ParseArchetypeComponents(gameCtx, n.ChildWithName("components_list"));
            gameCtx.CurrentArchetype = default;
        }

        private void ParseArchetypeComponents(ParserContext gameCtx, IAstNode ast)
        {
            void AddComponentToArchetype(ExtractedNode node)
            {
                var componentName = node.ChildWithName("component_name").GetStringWithoutQuote();
                var archetype = gameCtx.CurrentArchetype;
                var component = gameCtx.Game.GetComponent(componentName);
                archetype.TryAddComponent(component);
            }

            new ExtractedNode(ast)
                .On("components_list", node =>
                {
                    var compDefine = node.ChildWithName("component_define");
                    ParseArchetypeComponents(gameCtx, compDefine);
                    var tail = node.ChildWithName("components_list");
                    if (tail != null)
                        ParseArchetypeComponents(gameCtx, tail);
                })
                .On("component_define", node =>
                    ParseArchetypeComponents(gameCtx, node.Children.First()))
                .On("predefined_component", AddComponentToArchetype)
                .On("component_with_type", node =>
                {
                    ParseComponentDefine(gameCtx, node.Ast);
                    AddComponentToArchetype(node);
                })
                .Process();
        }

        void ParseAlias(ParserContext gameCtx, ExtractedNode node)
        {
            var text = node.ChildWithName("type_name").GetStringWithoutQuote();
            var replace = node.ChildWithName("full_name").GetStringWithoutQuote();
            gameCtx.AddAlias(text, replace);
        }

        void ParseComponentDefine(ParserContext gameCtx, IAstNode ast)
        {
            var extracted = new ExtractedNode(ast)
                .On("component_with_type", node =>
                {
                    var componentName = node.ChildWithName("component_name").GetStringWithoutQuote();
                    var componentType = node.ChildWithName("component_type").GetStringWithoutQuote();

                    gameCtx.TryAddComponent(componentName, componentType);
                });
        }

        #endregion
    }
}