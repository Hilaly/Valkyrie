using System;
using System.Reflection;

namespace Valkyrie.Defines
{
    abstract class NativeMemberDefine : IMemberDefine
    {
        readonly MemberInfo _native;

        public abstract ITypeDefine Type { get; set; }

        public string Name
        {
            get => _native.Name;
            set => throw new NotImplementedException();
        }

        protected NativeMemberDefine(MemberInfo native)
        {
            _native = native;
        }
    }

    class NativePropertyDefine : NativeMemberDefine, IPropertyDefine
    {
        public readonly PropertyInfo Native;

        public override ITypeDefine Type
        {
            get => new NativeTypeDefine(Native.PropertyType);
            set => throw new NotImplementedException();
        }

        public NativePropertyDefine(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            Native = propertyInfo;
        }
    }

    class NativeFieldDefine : NativeMemberDefine, IFieldDefine
    {
        public readonly FieldInfo Native;

        public override ITypeDefine Type
        {
            get => new NativeTypeDefine(Native.FieldType);
            set => throw new NotImplementedException();
        }

        public bool IsPublic
        {
            get => Native.IsPublic;
            set => throw new NotImplementedException();
        }

        public NativeFieldDefine(FieldInfo fi) : base(fi)
        {
            Native = fi;
        }
    }
}