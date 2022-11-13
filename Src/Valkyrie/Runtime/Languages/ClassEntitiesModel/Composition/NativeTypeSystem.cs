using System;

namespace Valkyrie.Composition
{
    public interface ISystemInfo
    {
        public string Name { get; }
    }
    
    class NativeTypeSystem : ISystemInfo
    {
        public Type Type;

        public string Name => Type.FullName.ToFullName();

        public NativeTypeSystem(Type type)
        {
            Type = type;
        }
    }
}