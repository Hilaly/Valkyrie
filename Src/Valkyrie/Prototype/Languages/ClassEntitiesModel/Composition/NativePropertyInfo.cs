using System.Reflection;

namespace Valkyrie.Composition
{
    public interface IComponentInfo
    {
        string Name { get; }
        
        string GetTypeName();
    }

    public interface IPropertyInfo
    {
        string Name { get; }
        
        string GetTypeName();
        bool IsWriteEnabled { get; }
        bool IsReadEnabled { get; }
    }

    class NativePropertyInfo : IPropertyInfo, IComponentInfo
    {
        public readonly PropertyInfo PropertyInfo;

        public NativePropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
        }

        public string Name => PropertyInfo.Name;

        public string GetTypeName() => PropertyInfo.PropertyType.FullName;

        public bool IsWriteEnabled => PropertyInfo.CanWrite;
        public bool IsReadEnabled => PropertyInfo.CanRead;
    }
}