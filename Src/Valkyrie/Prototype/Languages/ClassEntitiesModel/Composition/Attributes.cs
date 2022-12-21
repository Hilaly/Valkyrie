using System;
using System.Collections.Generic;

namespace Valkyrie.Composition
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class RequiredPropertyAttribute : Attribute
    {
        public IReadOnlyList<string> Properties { get; }

        public RequiredPropertyAttribute(params string[] requiredProperties) => Properties = requiredProperties;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ExcludePropertyAttribute : Attribute
    {
        public IReadOnlyList<string> Properties { get; }

        public ExcludePropertyAttribute(params string[] excludedProperties) => Properties = excludedProperties;
    }
}