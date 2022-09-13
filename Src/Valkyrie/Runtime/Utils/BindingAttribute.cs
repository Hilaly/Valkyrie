using System;

namespace Utils
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Interface,
        Inherited = false)]
    public class BindingAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AutoBindAttribute : Attribute
    {
        public string XPath { get; }
        public Type Adapter { get; set; }

        public AutoBindAttribute(string xPath)
        {
            XPath = xPath;
        }
    }
}