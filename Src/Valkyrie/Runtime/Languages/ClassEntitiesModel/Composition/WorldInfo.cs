using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Composition
{
    public interface IComponentInfo
    {
        string Name { get; }
        
        string GetTypeName();
    }

    class NativeTypeComponent : IComponentInfo
    {
        public Type Type;
        public string Name { get; set; }

        public string GetTypeName() => Type.FullName;
    }
    
    public interface IFeature
    {
        void Register(IWorldInfo worldInfo);
    }
    
    public class WorldInfo : IWorldInfo
    {
        public string Namespace { get; set; } = "Generated";
        
        public IReadOnlyList<IComponentInfo> GetComponents()
        {
            return new IComponentInfo[]
            {
                new NativeTypeComponent() { Name = "TestInt", Type = typeof(int) },
                new NativeTypeComponent() { Name = "TestInt2", Type = typeof(int) },
                new NativeTypeComponent() { Name = "TestFloat", Type = typeof(float) },
                new NativeTypeComponent() { Name = "TestMarker", Type = typeof(bool) },
                new NativeTypeComponent() { Name = "TestTimer", Type = typeof(ITimer) },
            };
        }
    }

    public interface IWorldInfo
    {
        string Namespace { get; }

        IReadOnlyList<IComponentInfo> GetComponents();
    }
}