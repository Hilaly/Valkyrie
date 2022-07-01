using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Di
{
    class ConcreteFactoryRegistration<T> : IConcreteTypeFactoryRegistration<T>, IRegistrationInfo,
        ISingletonRegistration<T>
    {
        private readonly Func<IContainer, IEnumerable<object>, T> _factory;
        private readonly HashSet<Type> _types = new HashSet<Type>();

        #region IRegistrationInfo

        public string Name { get; }
        public IEnumerable<Type> ResolvedTypes => _types;
        public InstantiationType InstantiationType { get; private set; }
        public Action<ResolvingArguments, object> OnActivationAction { get; private set; }
        public bool IsNonLazy { get; private set; }

        #endregion

        public ConcreteFactoryRegistration(Func<IContainer, IEnumerable<object>, T> factory, string name)
        {
            _factory = factory;
            Name = name;
        }

        #region IConcreteTypeFactoryRegistration

        public IConcreteTypeFactoryRegistration<T> As<TResolveType>()
        {
            if (!typeof(TResolveType).IsAssignableFrom(GetTypeInfo()))
                throw new InvalidCastException(
                    $"{GetTypeInfo().FullName} con not be converted to {typeof(TResolveType).FullName}");
            _types.Add(typeof(TResolveType));
            return this;
        }

        public IConcreteTypeFactoryRegistration<T> AsSelf()
        {
            _types.Add(GetTypeInfo());
            return this;
        }

        public IConcreteTypeFactoryRegistration<T> AsInterfaces()
        {
            foreach (var type in GetTypeInfo().GetInterfaces())
                _types.Add(type);
            return this;
        }

        public IConcreteTypeFactoryRegistration<T> AsInterfacesAndSelf()
        {
            return AsInterfaces().AsSelf();
        }

        public ISingletonRegistration<T> SingleInstance()
        {
            InstantiationType = InstantiationType.Single;
            return this;
        }

        public IConcreteTypeFactoryRegistration<T> InstancePerScope()
        {
            InstantiationType = InstantiationType.Scope;
            return this;
        }

        public IConcreteTypeFactoryRegistration<T> InstancePerDependency()
        {
            InstantiationType = InstantiationType.Dependency;
            return this;
        }

        #endregion

        #region ISingletonRegistration

        void ISingletonRegistration<T>.NonLazy()
        {
            IsNonLazy = true;
        }

        #endregion

        public Func<ResolvingArguments, object> GetInstanceFactory()
        {
            return (args) => _factory(args.Container, args.ResolvedArguments.Select(u => u.Argument));
        }

        public Type GetTypeInfo()
        {
            return typeof(T);
        }

        public IConcreteTypeFactoryRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback)
        {
            OnActivationAction = (arguments, instance) =>
                activationCallback(new ActivationContext<T>(arguments, (T)instance));
            return this;
        }
    }
}