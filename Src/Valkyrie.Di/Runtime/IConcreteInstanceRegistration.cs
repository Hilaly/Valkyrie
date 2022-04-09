using System;

namespace Valkyrie.Di
{
    public interface IConcreteInstanceRegistration<T> : IResolveRegistration<IConcreteInstanceRegistration<T>>,
        ISingletonRegistration<T>
    {
        IConcreteInstanceRegistration<T> OnActivation(Action<IActivationContext<T>> activationCallback);
    }
}