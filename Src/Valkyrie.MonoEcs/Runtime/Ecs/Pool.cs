using System;

namespace Valkyrie.Ecs
{
    internal interface IPool
    {
        bool RemoveById(int id);
    }

    class Pool<T> : IPool where T : struct
    {
        internal struct X
        {
            public int Id;
            public T Value;
        }

        private X[] _components = new X[10];
        private T _default = new T();
        private int _count;

        int FindIndex(int id, out bool exist)
        {
            for (var i = 0; i < _count; ++i)
            {
                if (_components[i].Id < id)
                    continue;
                exist = _components[i].Id == id;
                return i;
            }

            exist = false;
            return _count;
        }

        public Memory<X> All => new Memory<X>(_components, 0, _count);

        public ref T GetById(int id, out bool exist)
        {
            var index = FindIndex(id, out exist);
            if (exist)
                return ref _components[index].Value;
            return ref _default;
        }

        public ref T GetById(int id)
        {
            var index = FindIndex(id, out var exist);
            if (exist)
                return ref _components[index].Value;
            return ref _default;
        }

        public bool AddById(int id, T value)
        {
            if (_components.Length == _count)
                Resize();
            var index = FindIndex(id, out var exist);
            if (exist)
                _components[index].Value = value;
            else
                InsertAt(index, new X() { Id = id, Value = value });

            return !exist;
        }

        private void InsertAt(int inde, X value)
        {
            Array.Copy(_components, inde, _components, inde + 1, _count - inde);
            _count++;
            _components[inde] = value;
        }

        public bool RemoveById(int id)
        {
            var index = FindIndex(id, out var exist);
            if (exist)
            {
                Array.Copy(_components, index + 1, _components, index, _count - index - 1);
                _count--;
            }

            return exist;
        }

        private void Resize()
        {
            var temp = _components;
            Array.Resize(ref _components, _components.Length * 2);
        }
    }
}