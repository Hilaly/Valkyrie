using System;
using UnityEngine;

namespace Utils.Pool
{
    class Wrapper<T> : IPooledInstance<T> where T : Component
    {
        private readonly Action _disposeCall;
        public T Instance { get; }

        public Wrapper(Action disposeCall, T instance)
        {
            _disposeCall = disposeCall;
            Instance = instance;
        }

        public void Dispose() => _disposeCall();
    }
}