using System;

namespace Valkyrie.Di
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property)]
    public class SettingsAttribute : Attribute
    {
        public string Name { get; set; }
        public string JsonValue { get; set; }
    }
}