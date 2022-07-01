using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Di
{
    class ConcreteTypeRegistration<T> : IConcreteTypeRegistration<T>, IRegistrationInfo, ISingletonRegistration<T>
    {
        private readonly HashSet<Type> _types = new HashSet<Type>();

        #region IRegistrationInfo

        public string Name { get; }
        public IEnumerable<Type> ResolvedTypes => _types;
        public InstantiationType InstantiationType { get; private set; }
        public Action<ResolvingArguments, object> OnActivationAction { get; private set; }
        public bool IsNonLazy { get; private set; }

        #endregion

        public ConcreteTypeRegistration(string name)
        {
            Name = name;
        }

        #region IConcreteTypeRegistration

        public IConcreteTypeRegistration<T> As<TResolveType>()
        {
            if (!typeof(TResolveType).IsAssignableFrom(GetTypeInfo()))
                throw new InvalidCastException(
                    $"{GetTypeInfo().FullName} con not be converted to {typeof(TResolveType).FullName}");
            _types.Add(typeof(TResolveType));
            return this;
        }

        public IConcreteTypeRegistration<T> AsSelf()
        {
            _types.Add(GetTypeInfo());
            return this;
        }

        public IConcreteTypeRegistration<T> AsInterfaces()
        {
            foreach (var type in GetTypeInfo().GetInterfaces())
                _types.Add(type);
            return this;
        }

        public IConcreteTypeRegistration<T> AsInterfacesAndSelf()
        {
            return AsInterfaces().AsSelf();
        }

        public ISingletonRegistration<T> SingleInstance()
        {
            InstantiationType = InstantiationType.Single;
            return this;
        }

        public IConcreteTypeRegistration<T> InstancePerScope()
        {
            InstantiationType = InstantiationType.Scope;
            return this;
        }

        public IConcreteTypeRegistration<T> InstancePerDependency()
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
            var constructors = typeof(T).GetConstructors();
            if (constructors.Length == 1)
                return DiUtils.MakeFactory(constructors[0], GetTypeInfo());

            try
            {
                var concreteCtor = constructors.Single(ctor =>
                    ctor.GetCustomAttributes(typeof(InjectAttribute), false).Length != 0);
                return DiUtils.MakeFactory(concreteCtor, GetTypeInfo());
            }
            catch (Exception e)
            {
                throw new Exception(
                    $"Can not find single constructor with Inject attribute at type {GetTypeInfo().FullName}", e);
            }
        }

        public Type GetTypeInfo()
        {
            return typeof(T);
        }

        public IConcreteTypeRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback)
        {
            OnActivationAction = (arguments, instance) =>
                activationCallback(new ActivationContext<T>(arguments, (T)instance));
            return this;
        }
    }
}