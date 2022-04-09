using System;
using System.Collections.Generic;

namespace Valkyrie.Di
{
    class ResolvingArguments
    {
        public class ArgumentInfo
        {
            public readonly object Argument;
            public readonly object Creator;
            public readonly IEnumerable<Type> ResolvedAs;
            public readonly string Name;

            public ArgumentInfo(object argument, object creator, IEnumerable<Type> resolvedAs, string name)
            {
                Argument = argument;
                Creator = creator;
                ResolvedAs = resolvedAs;
                Name = name;
            }
        }

        public readonly Container Container;
        public readonly CompositeDisposable Disposable;
        public readonly List<ArgumentInfo> ResolvedArguments;

        public ResolvingArguments(Container container, CompositeDisposable disposable)
        {
            Container = container;
            Disposable = disposable;
            ResolvedArguments = new List<ArgumentInfo>
            {
                new ArgumentInfo(container, container, new[] { typeof(IContainer) }, null)
            };
        }
    }
}