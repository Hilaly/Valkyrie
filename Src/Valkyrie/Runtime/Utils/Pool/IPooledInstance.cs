using System;
using UnityEngine;

namespace Valkyrie.Utils.Pool
{
    public interface IPooledInstance<out T> : IDisposable where T : Component
    {
        T Instance { get; }
    }
}