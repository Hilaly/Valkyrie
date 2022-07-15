using System.Collections.Generic;
using System.Linq;

namespace GamePrototype
{
    public class ViewModel
    {
        
    }

    public class Entity
    {
        private List<object> _components = new List<object>();

        public T GetComponent<T>() => _components.OfType<T>().FirstOrDefault();

        public void AddComponent<T>(T component)
        {
            RemoveComponent<T>();
            _components.Add(component);
        }

        public void RemoveComponent<T>()
        {
            _components.RemoveAll(x => x is T);
        }

        public bool HasComponent<T>() => GetComponent<T>() != null;
    }
}