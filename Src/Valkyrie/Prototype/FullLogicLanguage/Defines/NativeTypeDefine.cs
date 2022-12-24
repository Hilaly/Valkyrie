using System;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Defines
{
    class NativeTypeDefine : ITypeDefine, IEquatable<NativeTypeDefine>
    {
        public readonly Type Native;

        public string Namespace
        {
            get => Native.Namespace;
            set => throw new NotImplementedException();
        }

        public string Name
        {
            get => Native.Name; 
            set => throw new NotImplementedException();
        }

        public bool IsValueType
        {
            get => Native.IsValueType;
            set => throw new NotImplementedException();
        }

        public bool IsClass
        {
            get => Native.IsClass;
            set => throw new NotImplementedException();
        }

        public bool IsPublic
        {
            get => Native.IsPublic;
            set => throw new NotImplementedException();
        }

        public NativeTypeDefine(Type native)
        {
            Native = native;
        }

        public ITypeDefine BaseType
        {
            get => Native.BaseType != default ? new NativeTypeDefine(Native.BaseType) : default;
            set => throw new NotImplementedException();
        }
        public IReadOnlyList<ITypeDefine> GetInterfaces()
        {
            return Native.GetInterfaces().Select(x => new NativeTypeDefine(x)).ToList();
        }

        public void TryAddInterface(ITypeDefine define)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<IMemberDefine> GetMembers()
        {
            return GetProperties().OfType<IMemberDefine>()
                .Union(GetFields())
                .ToList();
        }

        public IReadOnlyList<IPropertyDefine> GetProperties()
        {
            return Native.GetProperties().Select(x => new NativePropertyDefine(x)).ToList();
        }

        public IReadOnlyList<IFieldDefine> GetFields()
        {
            return Native.GetFields().Select(x => new NativeFieldDefine(x)).ToList();
        }

        public void TryAddField(IFieldDefine fieldDefine)
        {
            throw new NotImplementedException();
        }

        public void TryAddProperty(IPropertyDefine fieldDefine)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) => obj is NativeTypeDefine ntd && ntd.Native == Native;

        public bool Equals(NativeTypeDefine other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Native == other.Native;
        }

        public override int GetHashCode()
        {
            return (Native != null ? Native.GetHashCode() : 0);
        }

        public static bool operator ==(NativeTypeDefine left, NativeTypeDefine right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NativeTypeDefine left, NativeTypeDefine right)
        {
            return !Equals(left, right);
        }
    }
}