using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Valkyrie.Threading.Async;

namespace Valkyrie.Di
{
    [AttributeUsage(
        AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method |
        AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public bool IsOptional { get; set; }
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method |
                    AttributeTargets.Parameter,
        AllowMultiple = true, Inherited = true)]
    public class InjectOptionalAttribute : InjectAttribute
    {
        public InjectOptionalAttribute()
        {
            IsOptional = true;
        }
    }

    public interface IContainer : IDisposable
    {
        #region registration

        IConcreteInstanceRegistration<T> Register<T>(T instance);
        IConcreteInstanceRegistration<T> Register<T>(T instance, string name);

        IConcreteTypeFactoryRegistration<T> Register<T>(Func<T> factory);
        IConcreteTypeFactoryRegistration<T> Register<T>(Func<T> factory, string name);
        IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, T> factory);
        IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, T> factory, string name);
        IConcreteTypeFactoryRegistration<T> Register<T>(Func<IContainer, IEnumerable<object>, T> factory, string name);

        IConcreteTypeRegistration<T> Register<T>();
        IConcreteTypeRegistration<T> Register<T>(string name);

        IContainer RegisterLibrary(ILibrary library);

        #endregion

        #region Build

        IContainer Build();

        #endregion

        #region resolving

        IEnumerable<T> ResolveAll<T>();

        T Resolve<T>();
        T Resolve<T>(params object[] args);
        T Resolve<T>(string name, params object[] args);

        T TryResolve<T>();
        T TryResolve<T>(params object[] args);
        T TryResolve<T>(string name, params object[] args);

        bool CanResolve<T>();
        bool CanResolve<T>(string name);

        T Inject<T>(T target);
        T Inject<T>(T target, params object[] args);

        #endregion

        #region Child containers

        IContainer CreateChild();

        #endregion

        #region Resolving

        object Resolve(Type type);
        object Resolve(Type type, params object[] args);
        object Resolve(Type type, string name, params object[] args);

        object TryResolve(Type type);
        object TryResolve(Type type, params object[] args);
        object TryResolve(Type type, string name, params object[] args);

        #endregion
    }

    class NewContainer : IContainer
    {
        private readonly NewContainer _parentContainer;
        private readonly CompositeDisposable _compositeDisposable;

        private readonly List<IRegistrationInfo> _registrationInfos = new List<IRegistrationInfo>();

        private readonly Dictionary<Type, List<IContainerResolver>> _resolvers =
            new Dictionary<Type, List<IContainerResolver>>();

        public NewContainer()
        {
            _compositeDisposable = new CompositeDisposable();
        }

        private NewContainer(NewContainer parentContainer)
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

        public ResolvingArguments StartResolving(IEnumerable<object> additionalArgs)
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
            var result = new NewContainer(this);
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

    public interface ILibrary
    {
        void Register(IContainer container);
    }

    public interface ISingletonRegistration<T>
    {
        void NonLazy();
    }

    public interface IResolveRegistration<out T>
        where T : IResolveRegistration<T>
    {
        T As<TResolveType>();
        T AsSelf();
        T AsInterfaces();
        T AsInterfacesAndSelf();
    }

    public interface IConcreteInstanceRegistration<T> : IResolveRegistration<IConcreteInstanceRegistration<T>>,
        ISingletonRegistration<T>
    {
        IConcreteInstanceRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }

    public interface IConcreteTypeRegistration<T> : IResolveRegistration<IConcreteTypeRegistration<T>>
    {
        ISingletonRegistration<T> SingleInstance();
        IConcreteTypeRegistration<T> InstancePerScope();
        IConcreteTypeRegistration<T> InstancePerDependency();

        IConcreteTypeRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }

    public interface IConcreteTypeFactoryRegistration<T> : IResolveRegistration<IConcreteTypeFactoryRegistration<T>>
    {
        ISingletonRegistration<T> SingleInstance();
        IConcreteTypeFactoryRegistration<T> InstancePerScope();
        IConcreteTypeFactoryRegistration<T> InstancePerDependency();

        IConcreteTypeFactoryRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }

    enum InstantiationType
    {
        Undefined,
        Single,
        Scope,
        Dependency
    }

    interface IRegistrationInfo
    {
        string Name { get; }
        InstantiationType InstantiationType { get; }

        IEnumerable<Type> ResolvedTypes { get; }

        Func<ResolvingArguments, object> GetInstanceFactory();

        Type GetTypeInfo();

        Action<ResolvingArguments, object> OnActivationAction { get; }

        bool IsNonLazy { get; }
    }

    public interface IActivationContext<out T>
    {
        T Instance { get; }

        TK TryResolve<TK>();
        TK TryResolve<TK>(string name);
        IEnumerable<TK> ResolveAll<TK>();
    }

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

        public readonly NewContainer Container;
        public readonly ICompositeDisposable Disposable;
        public readonly List<ArgumentInfo> ResolvedArguments;

        public ResolvingArguments(NewContainer container, ICompositeDisposable disposable)
        {
            Container = container;
            Disposable = disposable;
            ResolvedArguments = new List<ArgumentInfo>
            {
                new ArgumentInfo(container, container, new[] { typeof(IContainer) }, null)
            };
        }
    }

    static class DiUtils
    {
        public static IEnumerable<Type> GetResolvedTypes(this Type type)
        {
            foreach (var @interface in type.GetInterfaces())
                yield return @interface;

            var temp = type;
            while (temp != null)
            {
                yield return temp;
                temp = temp.BaseType;
            }
        }

        public static object Invoke(this IContainer container, MethodInfo methodInfo, object instance,
            params object[] args)
        {
            var ra = ((NewContainer)container).StartResolving(args);
            return MakeMethodInvokeAction(methodInfo.DeclaringType, methodInfo)
                .Invoke(ra, instance);
        }

        private static bool TryGet(ResolvingArguments args, Type type, string name, out object result)
        {
            foreach (var argumentInfo in args.ResolvedArguments)
            {
                if (!argumentInfo.ResolvedAs.Contains(type) || argumentInfo.Name != name)
                    continue;

                result = argumentInfo.Argument;
                return true;
            }

            result = args.Container.TryResolve(args, type, name);
            if (result == null && type.IsConstructedGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                result = args.Container.ResolveAll(type.GetGenericArguments()[0]);
            return result != null;
        }

        private static readonly Dictionary<Type, Action<ResolvingArguments, object>> InjectActionsCash = new();

        public static Action<ResolvingArguments, object> MakeInjectionAction(Type instanceType)
        {
            if (InjectActionsCash.TryGetValue(instanceType, out var existAction))
                return existAction;

            var fields = GetFields(instanceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var fieldsNames = new string[fields.Length];
            var fieldOptional = new bool[fields.Length];
            for (var i = 0; i < fields.Length; ++i)
            {
                var attr = fields[i].GetCustomAttribute<InjectAttribute>(true);
                fieldsNames[i] = attr.Name;
                fieldOptional[i] = attr.IsOptional;
            }

            var properties = GetProperties(instanceType,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var propertiesNames = new string[properties.Length];
            var propertiesOptional = new bool[properties.Length];
            for (var i = 0; i < properties.Length; ++i)
            {
                var attr = properties[i].GetCustomAttribute<InjectAttribute>(true);
                propertiesNames[i] = attr.Name;
                propertiesOptional[i] = attr.IsOptional;
            }

            var methods = GetMethods(instanceType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            var methodInvokes = new Func<ResolvingArguments, object, object>[methods.Length];
            for (var i = 0; i < methods.Length; ++i)
                methodInvokes[i] = MakeMethodInvokeAction(instanceType, methods[i]);

            void InjectMethod(ResolvingArguments args, object instance)
            {
                for (var i = 0; i < fields.Length; ++i)
                    if (TryGet(args, fields[i].FieldType, fieldsNames[i], out var arg) || fieldOptional[i])
                        fields[i].SetValue(instance, arg);
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {fields[i].FieldType.FullName} when inject into field {fields[i].Name} of {instanceType.FullName}");

                for (var i = 0; i < properties.Length; ++i)
                    if (TryGet(args, properties[i].PropertyType, propertiesNames[i], out var arg) ||
                        propertiesOptional[i])
                        properties[i].SetValue(instance, arg, null);
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {properties[i].PropertyType.FullName} when inject into property {properties[i].Name} of {instanceType.FullName}");

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < methodInvokes.Length; ++i)
                    methodInvokes[i].Invoke(args, instance);
            }

            InjectActionsCash.Add(instanceType, existAction = InjectMethod);
            return existAction;
        }

        private static Func<ResolvingArguments, object, object> MakeMethodInvokeAction(Type instanceType,
            MethodInfo methodInfo)
        {
            var paramInfos = methodInfo.GetParameters();

            var names = new string[paramInfos.Length];
            var methodArgs = new object[paramInfos.Length];
            var methodOptional = new bool[paramInfos.Length];
            for (var i = 0; i < paramInfos.Length; ++i)
            {
                var attr = paramInfos[i].GetCustomAttribute<InjectAttribute>(true);
                names[i] = attr?.Name;
                methodOptional[i] = attr?.IsOptional ?? false;
            }

            object FactoryMethod(ResolvingArguments args, object instance)
            {
                for (var i = 0; i < paramInfos.Length; ++i)
                    if (TryGet(args, paramInfos[i].ParameterType, names[i], out var arg) || methodOptional[i])
                        methodArgs[i] = arg;
                    else
                        throw new Exception(
                            $"Can not resolve argument of type {paramInfos[i].ParameterType.FullName} when invoke {methodInfo.Name} of {instanceType.FullName}");

                return methodInfo.Invoke(instance, methodArgs);
            }

            return FactoryMethod;
        }

        public static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flags)
        {
            foreach (var propInfo in type.GetFields(flags))
                yield return propInfo;
            if (type.BaseType == null) yield break;
            foreach (var propInfo in GetFields(type.BaseType, BindingFlags.NonPublic | flags).Where(u => u.IsPrivate))
                yield return propInfo;
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type, BindingFlags flags)
        {
            foreach (var propInfo in type.GetProperties(flags))
                yield return propInfo;
            if (type.BaseType == null) yield break;
            foreach (var propInfo in GetProperties(type.BaseType, BindingFlags.NonPublic | flags))
                yield return propInfo;
        }

        private static IEnumerable<MethodInfo> GetMethods(Type type, BindingFlags flags)
        {
            foreach (var methodInfo in type.GetMethods(flags))
                yield return methodInfo;
            if (type.BaseType == null) yield break;
            foreach (var methodInfo in GetMethods(type.BaseType, BindingFlags.NonPublic | flags))
                yield return methodInfo;
        }

        public static Func<ResolvingArguments, object> MakeFactory(ConstructorInfo ctorInfo, Type instanceType)
        {
            var paramInfos = ctorInfo.GetParameters();

            var names = new string[paramInfos.Length];
            var ctorArgs = new object[paramInfos.Length];
            var ctorOptional = new bool[paramInfos.Length];
            for (var i = 0; i < paramInfos.Length; ++i)
            {
                var attr = paramInfos[i].GetCustomAttribute<InjectAttribute>(true);
                names[i] = attr?.Name;
                ctorOptional[i] = attr?.IsOptional ?? false;
            }

            object FactoryMethod(ResolvingArguments args)
            {
                for (var i = 0; i < paramInfos.Length; ++i)
                {
                    ctorArgs[i] = null;
                    var paramType = paramInfos[i].ParameterType;
                    if (!TryGet(args, paramType, names[i], out var arg) && !ctorOptional[i])
                        throw new Exception(
                            $"Can not resolve argument of type {paramType.FullName} when create instance of {instanceType.FullName}");
                    ctorArgs[i] = arg;
                }

                var result = ctorInfo.Invoke(ctorArgs);

                for (var i = 0; i < paramInfos.Length; ++i)
                    ctorArgs[i] = null;

                return result;
            }

            return FactoryMethod;
        }
    }

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

    interface IContainerResolver
    {
        string Name { get; }
        object Resolve(ResolvingArguments args);
    }

    abstract class BaseResolver : IContainerResolver
    {
        private readonly Action<ResolvingArguments, object> _onActivationCall;
        public string Name { get; }
        private Func<ResolvingArguments, object> Factory { get; }
        private Action<ResolvingArguments, object> Method { get; }
        public Type TypeInfo { get; }
        public bool IsDisposable { get; }
        private InstantiationType InstanceType { get; }
        private IEnumerable<Type> Types { get; }

        protected BaseResolver(IRegistrationInfo registrationInfo)
        {
            Name = registrationInfo.Name;
            TypeInfo = registrationInfo.GetTypeInfo();
            Factory = registrationInfo.GetInstanceFactory();
            Types = registrationInfo.ResolvedTypes;
            IsDisposable =
                typeof(IDisposable).IsAssignableFrom(registrationInfo
                    .GetTypeInfo()); // registrationInfo.ResolvedTypes.Contains(typeof(IDisposable));
            InstanceType = registrationInfo.InstantiationType;
            Method = DiUtils.MakeInjectionAction(TypeInfo);
            _onActivationCall = registrationInfo.OnActivationAction;
        }

        public virtual object Resolve(ResolvingArguments args)
        {
            //Instantiate
            var result = Factory(args);
            //Inject
            Method(args, result);
            //On activation
            _onActivationCall?.Invoke(args, result);

            if (InstanceType != InstantiationType.Dependency)
                args.ResolvedArguments.Add(new ResolvingArguments.ArgumentInfo(result, this, Types, Name));
            if (IsDisposable)
                args.Disposable.Add((IDisposable)result);

            return result;
        }
    }

    class SingleInstanceResolver : BaseResolver
    {
        private object _createdInstance;

        public SingleInstanceResolver(IRegistrationInfo registrationInfo) : base(registrationInfo)
        {
        }

        public override object Resolve(ResolvingArguments args)
        {
            return _createdInstance ?? (_createdInstance = base.Resolve(args));
        }
    }

    class ScopeInstanceResolver : BaseResolver
    {
        public ScopeInstanceResolver(IRegistrationInfo registrationInfo) : base(registrationInfo)
        {
        }
    }

    class DependencyInstanceResolver : BaseResolver
    {
        public DependencyInstanceResolver(IRegistrationInfo registrationInfo) : base(registrationInfo)
        {
        }
    }
}