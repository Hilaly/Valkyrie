using System;
using System.Collections.Generic;

namespace Valkyrie.Di
{
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
}