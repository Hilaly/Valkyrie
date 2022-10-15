using System;

namespace Meta
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Struct)]
    public class IsValidBindingTypeAttribute : Attribute
    {}
}