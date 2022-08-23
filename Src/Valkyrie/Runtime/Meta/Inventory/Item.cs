using System.Collections.Generic;

namespace Meta.Inventory
{
    public class Item
    {
        public string Id;
        public string TypeId;

        public readonly Dictionary<string, string> Attributes = new Dictionary<string, string>();
    }
}