using System;
using System.Collections.Generic;
using System.Text;

namespace Valkyrie.Grammar
{
    class NonTerminalNode : IAstNode
    {
        public NonTerminalNode(string name, IEnumerable<IAstNode> nodes)
        {
            Name = name;
            Nodes = new List<IAstNode>(nodes);
        }

        public string Name { get; }
        public IEnumerator<IAstNode> EnumerateTerminalNodes()
        {
            foreach (var node in Nodes)
            {
                var enumerator = node.EnumerateTerminalNodes();
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        public List<IAstNode> GetChildren() => Nodes;

        public List<IAstNode> Nodes { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(Name);
            foreach (var astNode in Nodes)
            foreach (var nodeSubString in astNode.ToString().Replace("\r", "").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries))
                sb.Append("  ").AppendLine(nodeSubString.Replace("\r", ""));
            return sb.ToString().Replace("\r", "");
        }
    }
}