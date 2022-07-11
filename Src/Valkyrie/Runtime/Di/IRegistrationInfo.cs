using System;
using System.Collections.Generic;

namespace Valkyrie.Di
{
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
}