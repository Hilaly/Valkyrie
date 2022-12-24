using System;
using Valkyrie.Defines;

namespace Valkyrie
{
    public class TypeReference : IEquatable<TypeReference>
    {
        public ITypeDefine DefinedType;
        public IBaseDescription BaseDescription;

        public string GetTypeString()
        {
            return DefinedType?.GetFullName()
                   ?? BaseDescription.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is TypeReference tr && Equals(tr);
        }

        public bool Equals(TypeReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(DefinedType, other.DefinedType) && Equals(BaseDescription, other.BaseDescription);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DefinedType, BaseDescription);
        }

        public static bool operator ==(TypeReference left, TypeReference right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TypeReference left, TypeReference right)
        {
            return !Equals(left, right);
        }
    }
}