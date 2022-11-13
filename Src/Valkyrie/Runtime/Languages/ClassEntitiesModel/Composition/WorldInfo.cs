using UnityEngine;

namespace Valkyrie.Composition
{
    public interface IFeature
    {
        void Register(IWorldInfo worldInfo);
    }
    
    public class WorldInfo : IWorldInfo
    {
        public string Namespace { get; set; } = "Generated";
    }

    public interface IWorldInfo
    {
        string Namespace { get; }
    }
}