using System;

namespace Valkyrie.Di
{
    public interface IConcreteTypeFactoryRegistration<T> : IResolveRegistration<IConcreteTypeFactoryRegistration<T>>
    {
        ISingletonRegistration<T> SingleInstance();
        IConcreteTypeFactoryRegistration<T> InstancePerScope();
        IConcreteTypeFactoryRegistration<T> InstancePerDependency();

        IConcreteTypeFactoryRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }
}