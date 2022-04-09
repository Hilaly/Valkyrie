using System;

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
}