using System.Collections.Generic;
using System.Linq;

namespace Meta.Inventory
{
    public static class InventoryExtension
    {
        public static IEnumerable<T> Filter<T>(this IInventory inventory) where T : ItemWrapper, new()
        {
            var typeId = typeof(T).Name;
            return inventory.GetAll().Where(x => x.TypeId == typeId).Select(Wrap<T>);
        }

        public static T Wrap<T>(this Item item) where T : ItemWrapper, new()
        {
            return new T() { Item = item };
        }

        public static T Wrap<T>(this IInventory inventory, string id) where T : ItemWrapper, new()
        {
            var item = inventory.Get(id);
            if (item == null)
                return default;
            if (item.TypeId != typeof(T).Name)
                return default;
            return item.Wrap<T>();
        }

        public static void Add<T>(this IInventory inventory, T itemWrapper) where T : ItemWrapper
        {
            inventory.Add(itemWrapper.Item);
        }

        public static void Set(this Item item, string name, string value) 
            => item.Attributes[name] = value;

        public static void Set(this Item item, string name, long value) 
            => item.Attributes[name] = value.ToString();
        public static void Set(this Item item, string name, int value) 
            => item.Attributes[name] = value.ToString();
        public static void Set(this Item item, string name, float value) 
            => item.Attributes[name] = value.ToString("F4");
        public static void Set(this Item item, string name, bool value) 
            => item.Attributes[name] = value.ToString();

        public static long Get(this Item item, string name, long defaultValue = 0)
            => item.Attributes.TryGetValue(name, out var temp) ? long.Parse(temp) : defaultValue;
        public static int Get(this Item item, string name, int defaultValue = 0)
            => item.Attributes.TryGetValue(name, out var temp) ? int.Parse(temp) : defaultValue;
        public static float Get(this Item item, string name, float defaultValue = 0)
            => item.Attributes.TryGetValue(name, out var temp) ? float.Parse(temp) : defaultValue;
        public static bool Get(this Item item, string name, bool defaultValue = false)
            => item.Attributes.TryGetValue(name, out var temp) ? bool.Parse(temp) : defaultValue;
    }
}