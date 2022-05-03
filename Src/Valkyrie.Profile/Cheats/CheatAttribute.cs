using System;

namespace Valkyrie.Cheats
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CheatAttribute : Attribute
    {
        public string ItemName { get; }
        public int Priority { get; }

        public CheatAttribute(string itemName)
        {
            ItemName = itemName;
        }

        public CheatAttribute(string itemName, int priority)
        {
            ItemName = itemName;
            Priority = priority;
        }
    }
}