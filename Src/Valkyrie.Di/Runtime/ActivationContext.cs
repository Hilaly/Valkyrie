using System.Collections.Generic;

namespace Valkyrie.Di
{
    class ActivationContext<T> : IActivationContext<T>
    {
        private readonly ResolvingArguments _args;

        public T Instance { get; }

        public TK TryResolve<TK>()
        {
            return TryResolve<TK>(null);
        }

        public TK TryResolve<TK>(string name)
        {
            return (TK)_args.Container.TryResolve(_args, typeof(TK), name);
        }

        public IEnumerable<TK> ResolveAll<TK>()
        {
            return _args.Container.ResolveAll<TK>();
        }

        public ActivationContext(ResolvingArguments args, T instance)
        {
            _args = args;
            Instance = instance;
        }
    }
}