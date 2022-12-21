using System;

namespace Valkyrie.Model
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExportPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}