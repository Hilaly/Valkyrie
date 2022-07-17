using System.Collections.Generic;

namespace Valkyrie.UserInput
{
    class GenericInnerListOwner<T>
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
    }
}