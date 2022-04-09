using System;
using System.Collections.Generic;

namespace Valkyrie.Di
{
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
}