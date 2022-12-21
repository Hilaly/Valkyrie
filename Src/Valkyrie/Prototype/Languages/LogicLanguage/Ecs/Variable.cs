using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Valkyrie.Language.Ecs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Variable
    {
        private enum VarType
        {
            Null,
            Float,
            Int,
            Bool,
            String
        }

        [FieldOffset(0)] public float fValue;
        [FieldOffset(0)] public readonly int iValue;
        [FieldOffset(0)] public IntPtr sValue;

        [FieldOffset(8)] public readonly byte type;

        #region Ctors

        public Variable(Variable other) : this()
        {
            this.iValue = other.iValue;
            this.type = other.type;
        }

        public Variable(float fValue) : this()
        {
            this.fValue = fValue;
            type = (byte)VarType.Float;
        }

        public Variable(int iValue) : this()
        {
            this.iValue = iValue;
            type = (byte)VarType.Int;
        }

        public Variable(bool bValue) : this()
        {
            this.iValue = bValue ? 1 : 0;
            type = (byte)VarType.Bool;
        }

        public Variable(string sValue) : this()
        {
            this.sValue = Marshal.StringToHGlobalUni(string.Intern(sValue));
            type = (byte)VarType.String;
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(Variable other)
        {
            return type switch
            {
                (int)VarType.Null => other.type == type,
                (int)VarType.Bool => other.type == type && iValue == other.iValue,
                (int)VarType.Float => (other.type == type || other.type == (int)VarType.Int) && AsFloat() == other.AsFloat(),
                (int)VarType.Int => (other.type == type && iValue == other.iValue) || (other.type == (int)VarType.Float && AsFloat() == other.AsFloat()),
                (int)VarType.String => other.type == type && AsString() == other.AsString(),
                _ => iValue == other.iValue
            };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(iValue, (int)type);
        }

        public static bool operator ==(Variable left, Variable right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Variable left, Variable right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region TypeCheck

        public bool IsBool() => type == (byte)VarType.Bool;
        public bool IsInt() => type == (byte)VarType.Int;
        public bool IsFloat() => type == (byte)VarType.Float;
        public bool IsString() => type == (byte)VarType.String;
        public bool IsNull() => type == (byte)VarType.Null;

        #endregion

        #region Conversions

        public float AsFloat() => IsFloat() ? fValue : (float)AsInt();// throw new InvalidCastException();
        public int AsInt() => IsInt() ? iValue : throw new InvalidCastException();
        public bool AsBool() => IsBool() ? (iValue != 0) : throw new InvalidCastException();
        public string AsString() => IsString() ? Marshal.PtrToStringUni(sValue) : throw new InvalidCastException();

        #endregion

        #region Static consts

        public static readonly Variable True = new Variable(true);
        public static readonly Variable False = new Variable(false);
        public static readonly Variable Null = new Variable();

        #endregion

        public override string ToString()
        {
            return (VarType)type switch
            {
                VarType.Null => "null",
                VarType.Float => AsFloat().ToString(CultureInfo.InvariantCulture),
                VarType.Int => AsInt().ToString(),
                VarType.Bool => AsBool().ToString(),
                VarType.String => AsString(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        #region Conversions

        public static implicit operator Variable(float value) => new Variable(value);
        public static implicit operator Variable(int value) => new Variable(value);
        public static implicit operator Variable(bool value) => new Variable(value);
        public static implicit operator Variable(string value) => new Variable(value);

        #endregion
    }
}