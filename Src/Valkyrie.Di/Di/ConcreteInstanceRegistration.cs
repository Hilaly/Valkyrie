using System;
using System.Collections.Generic;

namespace Valkyrie.Di
{
    class ConcreteInstanceRegistration<T> : IConcreteInstanceRegistration<T>, IRegistrationInfo
    {
        private readonly HashSet<Type> _types = new HashSet<Type>();
        private readonly T _instance;

        #region IRegistrationInfo

        public string Name { get; }
        public IEnumerable<Type> ResolvedTypes => _types;
        public InstantiationType InstantiationType => InstantiationType.Single;
        public Action<ResolvingArguments, object> OnActivationAction { get; private set; }
        public bool IsNonLazy { get; private set; }

        #endregion

        public ConcreteInstanceRegistration(T instance, string name)
        {
            _instance = instance;
            Name = name;
        }

        #region IConcreteInstanceRegistration

        public IConcreteInstanceRegistration<T> As<TResolveType>()
        {
            if (!typeof(TResolveType).IsAssignableFrom(GetTypeInfo()))
                throw new InvalidCastException(
                    $"{GetTypeInfo().FullName} con not be converted to {typeof(TResolveType).FullName}");
            _types.Add(typeof(TResolveType));
            return this;
        }

        public IConcreteInstanceRegistration<T> AsSelf()
        {
            _types.Add(GetTypeInfo());
            return this;
        }

        public IConcreteInstanceRegistration<T> AsInterfaces()
        {
            foreach (var type in GetTypeInfo().GetInterfaces())
                _types.Add(type);
            return this;
        }

        public IConcreteInstanceRegistration<T> AsInterfacesAndSelf()
        {
            return AsInterfaces().AsSelf();
        }

        #endregion

        public Func<ResolvingArguments, object> GetInstanceFactory()
        {
            var temp = _instance;
            return args => temp;
        }

        public Type GetTypeInfo()
        {
            return _instance.GetType();
        }

        public IConcreteInstanceRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback)
        {
            OnActivationAction = (arguments, instance) =>
                activationCallback(new ActivationContext<T>(arguments, (T)instance));
            return this;
        }

        public void NonLazy()
        {
            IsNonLazy = true;
        }
    }
}