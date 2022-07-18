using System;
using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto.PoolProto
{
    //TODO: extract interface
    public class Pool<T>
    {
        private readonly List<T> _pool = new();
        private readonly Func<T> _newMethod;
        private readonly Action<T> _storeMethod;

        public Pool(Func<T> newMethod, Action<T> storeMethod)
        {
            _newMethod = newMethod;
            _storeMethod = storeMethod;
        }

        public T Get()
        {
            if (_pool.Count > 0)
            {
                var index = _pool.Count - 1;
                var temp = _pool[index];
                _pool.RemoveAt(index);
                return temp;
            }

            return _newMethod();
        }

        public void Store(T value)
        {
            _pool.Add(value);
            _storeMethod(value);
        }
    }
}