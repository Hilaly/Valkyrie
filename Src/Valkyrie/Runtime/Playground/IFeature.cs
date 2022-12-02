using System;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public interface IFeature : ILibrary
    {
        void Install(IWorldController world);
    }

    public interface IComponent
    {
        IEntity Entity { get; }
    }

    public interface ISystem
    {
        void Simulate(float dt);
    }
}