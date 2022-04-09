using System.Collections.Generic;

namespace Valkyrie.Di
{
    public interface IActivationContext<out T>
    {
        T Instance { get; }

        TK TryResolve<TK>();
        TK TryResolve<TK>(string name);
        IEnumerable<TK> ResolveAll<TK>();
    }
}