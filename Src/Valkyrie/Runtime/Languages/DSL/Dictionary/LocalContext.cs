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
        private Dictionary<string, LocalContext> _children = new();

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

        public Dictionary<string, string> GetArgs()
        {
            var r = new Dictionary<string, string>();
            if (_parent != null)
                foreach (var arg in _parent.GetArgs())
                    r[arg.Key] = arg.Value;
            foreach (var arg in _args)
                r[arg.Key] = arg.Value;
            return r;
        }

        public void AddChild(string treeName, LocalContext localContext)
        {
            _children[treeName] = localContext;
        }

        public void ReplaceFrom(LocalContext o)
        {
            this._args = o._args;
            this._parent = o._parent;
            this._children = o._children;
            this.Actions = o.Actions;
            foreach (var child in _children.Values) child._parent = this;
        }
    }
}