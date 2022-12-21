using System.Collections.Generic;

namespace Valkyrie
{
    public class KeyListCollection<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public void Connect(TKey key, TValue value)
        {
            if (TryGetValue(key, out var list))
                list.Add(value);
            else
                Add(key, new List<TValue>() { value });
        }

        public void Disconnect(TKey key, TValue value)
        {
            if (TryGetValue(key, out var list))
                list.Remove(value);
        }
    }
}