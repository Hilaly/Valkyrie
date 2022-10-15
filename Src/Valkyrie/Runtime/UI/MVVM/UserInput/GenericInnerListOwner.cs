using System.Collections;
using System.Collections.Generic;

namespace Valkyrie.UserInput
{
    public class GenericInnerListOwner<T> : IEnumerable<T>
    {
        protected readonly List<T> Values = new List<T>();

        public void Add(T instance)
        {
            Values.Add(instance);
        }

        public void Remove(T instance)
        {
            Values.Remove(instance);
        }

        public IEnumerator<T> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}