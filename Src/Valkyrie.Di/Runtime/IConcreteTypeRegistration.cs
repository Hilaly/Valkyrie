using System;

namespace Valkyrie.Di
{
    public interface IConcreteTypeRegistration<T> : IResolveRegistration<IConcreteTypeRegistration<T>>
    {
        ISingletonRegistration<T> SingleInstance();
        IConcreteTypeRegistration<T> InstancePerScope();
        IConcreteTypeRegistration<T> InstancePerDependency();

        IConcreteTypeRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }
}