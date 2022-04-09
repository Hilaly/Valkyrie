using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Di
{
    public class Container : IContainer
    {
        private readonly Container _parentContainer;
        private readonly CompositeDisposable _compositeDisposable;

        private readonly List<IRegistrationInfo> _registrationInfos = new List<IRegistrationInfo>();

        private readonly Dictionary<Type, List<IContainerResolver>> _resolvers =
            new Dictionary<Type, List<IContainerResolver>>();

        public Container()
        {
            _compositeDisposable = new CompositeDisposable();
        }

        private Container(Container parentContainer)
        {
            _parentContainer = parentContainer;
            _compositeDisposable = new CompositeDisposable();
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public IConcreteInstanceRegistration<T> Register<T>(T instance)
        {
            return Register(instance, null);
        }

        public IConcreteInstanceRegistration<T> Register<T>(T instance, string name)
        {
            var result = new ConcreteInstanceRegistration<T>(instance, name);
            lock (_registrationInfos)
                _registrationInfos.Add(result);
            return result;
        }

        public IConcreteTypeFactoryRegistration<T> Register<T>(Func<T> factory)
        {
            return Register((container, args) => factory(), null);
        }

        public IConcreteTypeFactoryRegistration<T> Register<T>(Func<T> factory, string name)
        {
            return Register((container, args) => factory(), name);
        }

        public IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, T> factory)
        {
            return Register((container, args) => factory(container), null);
        }

        public IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, T> factory, string name)
        {
            return Register((container, args) => factory(container), name);
        }

        public IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, IEnumerable<object>, T> factory,
            string name)
        {
            var result = new ConcreteFactoryRegistration<T>(factory, name);
            lock (_registrationInfos)
                _registrationInfos.Add(result);
            return result;
        }

        public IConcreteTypeRegistration<T> Register<T>()
        {
            return Register<T>((string)null);
        }

        public IConcreteTypeRegistration<T> Register<T>(string name)
        {
            var result = new ConcreteTypeRegistration<T>(name);
            lock (_registrationInfos)
                _registrationInfos.Add(result);
            return result;
        }

        public IContainer RegisterLibrary(ILibrary library)
        {
            library.Register(this);
            return this;
        }

        public IContainer Build()
        {
            Register(this).As<IContainer>();

            var nonLazy = new List<IContainerResolver>();
            lock (_registrationInfos)
            {
                foreach (var registrationInfo in _registrationInfos)
                {
                    IContainerResolver resolver;
                    switch (registrationInfo.InstantiationType)
                    {
                        case InstantiationType.Undefined:
                            throw new Exception(
                                $"InstantiationType for type '{registrationInfo.GetTypeInfo().FullName}' is undefined. Use SingleInstance, InstancePerScope or InstancePerDependency");
                        case InstantiationType.Single:
                            resolver = new SingleInstanceResolver(registrationInfo);
                            break;
                        case InstantiationType.Scope:
                            resolver = new ScopeInstanceResolver(registrationInfo);
                            break;
                        case InstantiationType.Dependency:
                            resolver = new DependencyInstanceResolver(registrationInfo);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (registrationInfo.IsNonLazy)
                        nonLazy.Add(resolver);

                    if (!registrationInfo.ResolvedTypes.Any())
                        throw new Exception(
                            $"Resolved types for type '{registrationInfo.GetTypeInfo().FullName}' is empty. Use As<>, AsSelf, AsInterfaces or AsInterfacesAndSelf");

                    foreach (var resolvedType in registrationInfo.ResolvedTypes)
                    {
                        if (!_resolvers.TryGetValue(resolvedType, out var collection))
                            _resolvers.Add(resolvedType, collection = new List<IContainerResolver>());
                        collection.Add(resolver);
                    }
                }

                _registrationInfos.Clear();
            }

            if (nonLazy.Count > 0)
            {
                var args = StartResolving(null);
                foreach (var resolver in nonLazy)
                    resolver.Resolve(args);
            }

            return this;
        }

        internal ResolvingArguments StartResolving(IEnumerable<object> additionalArgs)
        {
            var ra = new ResolvingArguments(this, _compositeDisposable);
            if (additionalArgs != null)
                ra.ResolvedArguments.AddRange(additionalArgs.Select(u =>
                    new ResolvingArguments.ArgumentInfo(u, null, u.GetType().GetResolvedTypes(), null)));
            return ra;
        }

        public IEnumerable ResolveAll(Type type)
        {
            var resultType = typeof(HashSet<>).MakeGenericType(type);
            var result = Activator.CreateInstance(resultType);
            var addMethod = resultType.GetMethod("Add", new Type[] { type });

            if (_resolvers.TryGetValue(type, out var resolvers))
            {
                var args = StartResolving(null);
                foreach (var resolver in resolvers)
                    addMethod.Invoke(result, new[] { resolver.Resolve(args) });
            }

            if (_parentContainer != null)
                foreach (var tempResult in _parentContainer.ResolveAll(type))
                    addMethod.Invoke(result, new[] { tempResult });

            return (IEnumerable)result;
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            IEnumerable<T> Parent() =>
                _parentContainer != null ? _parentContainer.ResolveAll<T>() : Enumerable.Empty<T>();

            if (!_resolvers.TryGetValue(typeof(T), out var resolvers))
                return Parent();

            var args = StartResolving(null);
            return resolvers.Select(u => (T)u.Resolve(args)).Concat(Parent());
        }

        public T Resolve<T>()
        {
            return Resolve<T>((string)null);
        }

        public T Resolve<T>(params object[] args)
        {
            return Resolve<T>(null, args);
        }

        public T Resolve<T>(string name, params object[] args)
        {
            var result = TryResolve<T>(name, args);
            if (ReferenceEquals(result, default(T)))
                throw new Exception($"Could not resolve {typeof(T).FullName} with name '{name ?? string.Empty}'");
            return result;
        }

        public T TryResolve<T>()
        {
            return TryResolve<T>((string)null);
        }

        public T TryResolve<T>(params object[] args)
        {
            return TryResolve<T>(null, args);
        }

        public T TryResolve<T>(string name, params object[] args)
        {
            return (T)TryResolve(StartResolving(args), typeof(T), name);
        }

        public bool CanResolve<T>()
        {
            return CanResolve<T>(null);
        }

        public bool CanResolve<T>(string name)
        {
            return GetResolver(typeof(T), name) != null ||
                   _parentContainer != null && _parentContainer.CanResolve<T>(name);
        }

        public T Inject<T>(T target)
        {
            var method = DiUtils.MakeInjectionAction(target.GetType());
            method.Invoke(StartResolving(null), target);
            return target;
        }

        public T Inject<T>(T target, params object[] args)
        {
            var method = DiUtils.MakeInjectionAction(target.GetType());
            method.Invoke(StartResolving(args), target);
            return target;
        }

        IContainerResolver GetResolver(Type type, string name)
        {
            return _resolvers.TryGetValue(type, out var resolvers)
                ? resolvers.FirstOrDefault(u => u.Name == name)
                : null;
        }

        public IContainer CreateChild()
        {
            var result = new Container(this);
            _compositeDisposable.Add(result);
            return result;
        }

        public object Resolve(Type type, string name)
        {
            return TryResolve(StartResolving(null), type, name);
        }

        public object Resolve(Type type)
        {
            return Resolve(type, (string)null, null);
        }

        public object Resolve(Type type, params object[] args)
        {
            return Resolve(type, (string)null, args);
        }

        public object Resolve(Type type, string name, params object[] args)
        {
            var result = TryResolve(type, name, args);
            if (ReferenceEquals(result, default))
                throw new Exception($"Could not resolve {type.FullName} with name '{name ?? string.Empty}'");
            return result;
        }

        public object TryResolve(Type type)
        {
            return TryResolve(type, null, null);
        }

        public object TryResolve(Type type, params object[] args)
        {
            return TryResolve(type, null, args);
        }

        public object TryResolve(Type type, string name, params object[] args)
        {
            return TryResolve(StartResolving(args), type, name);
        }

        internal object TryResolve(ResolvingArguments args, Type type, string name)
        {
            var resolver = GetResolver(type, name);
            if (resolver == null)
                return _parentContainer?.TryResolve(args, type, name);

            if (args == null)
                args = StartResolving(null);

            return resolver.Resolve(args);
        }
    }
}