using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.DSL.Actions;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Dictionary
{
    public class LocalContext
    {
        private Dictionary<string, string> _args = new();
        private LocalContext _parent;
        private Dictionary<string, List<LocalContext>> _children = new();

        internal List<IDslAction> Actions;

        public void SetValue(string name, string value)
        {
            _args[name] = value;
        }

        public override string ToString() => $"Vars: {_args.Select(x => $"{x.Key}={x.Value}").Join(",")}";

        public LocalContext()
        {
        }

        public LocalContext(LocalContext other)
        {
            _parent = other;
            if (other.Actions != null)
                Actions = new List<IDslAction>(other.Actions);
        }

        public Dictionary<string, string> GetLocalVariables()
        {
            var r = new Dictionary<string, string>();
            if (_parent != null)
                foreach (var arg in _parent.GetLocalVariables())
                    r[arg.Key] = arg.Value;
            foreach (var arg in _args)
                r[arg.Key] = arg.Value;
            return r;
        }

        public void AddChild(string treeName, LocalContext localContext)
        {
            if (!_children.TryGetValue(treeName, out var list))
                _children.Add(treeName, list = new List<LocalContext>());
            list.Add(localContext);
        }

        public void ReplaceFrom(LocalContext o)
        {
            _args = o._args;
            _parent = o._parent;
            _children = o._children;
            Actions = o.Actions;
            foreach (var child in _children.SelectMany(x => x.Value))
                child._parent = this;
        }

        public IEnumerable<LocalContext> GetChildren(string treeName) => 
            _children.TryGetValue(treeName, out var list) ? list : Enumerable.Empty<LocalContext>();

        public void PushVariableUp(string varName)
        {
            if(_parent != null && _args.TryGetValue(varName, out var value))
                _parent.SetValue(varName, value);
        }
    }
}