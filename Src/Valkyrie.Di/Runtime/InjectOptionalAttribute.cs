using System;

namespace Valkyrie.Di
{
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
}