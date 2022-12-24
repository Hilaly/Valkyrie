using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Valkyrie.Grammar;

namespace Valkyrie
{
    public class ExtractedNode
    {
        public IAstNode Ast { get; }
        public IReadOnlyList<IAstNode> Children { get; }
        
        public bool LogTrace { get; set; }

        private bool _processed;

        public ExtractedNode(IAstNode ast)
        {
            Ast = ast;
            Children = Ast.GetChildren(true);
        }

        public IAstNode ChildWithName(string name) => Children.FirstOrDefault(x => x.Name == name || x.Name == $"<{name}>");

        public IReadOnlyList<IAstNode> FilterByName(string name) =>
            Filter(x => x.Name == name || x.Name == $"<{name}>");

        public IReadOnlyList<IAstNode> Filter(Func<IAstNode, bool> filter) => Unpack(Ast, filter);

        static List<IAstNode> Unpack(IAstNode node, Func<IAstNode, bool> filter)
        {
            var r = new List<IAstNode>();
            if (filter(node))
                r.Add(node);
            else
                foreach (var astNode in node.GetChildren())
                    r.AddRange(Unpack(astNode, filter));
            return r;
        }

        public ExtractedNode On(string nodeName, Action<ExtractedNode> call)
        {
            if (!_processed && (Ast.Name == nodeName || Ast.Name == $"<{nodeName}>"))
            {
                _processed = true;
                if(LogTrace)
                    Debug.Log($"[FLL] process: {this}");
                call(this);
            }

            return this;
        }

        public ExtractedNode Process()
        {
            if (!_processed)
            {
                Debug.LogError($"[FLL] process error: {this}");
                throw new GrammarCompileException(Ast, $"Unsupported node name {Ast.Name}");
            }
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Ast.Name);
            foreach (var node in Children) 
                sb.AppendLine($"\t{node.Name}");
            return sb.ToString();
        }

        public void Log()
        {
            Debug.Log($"[FLL]: {this}");
        }
    }
}