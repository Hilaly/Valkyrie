using System;

namespace Valkyrie.MVVM.Adapters
{
    public class ToFloatAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return type == typeof(int)
                   || type == typeof(long)
                   || type == typeof(short)
                   || type == typeof(byte);
        }

        public Type GetResultType()
        {
            return typeof(float);
        }

        public object Convert(object source)
        {
            switch (source)
            {
                case int i:
                    return (float) i;
                case byte b:
                    return (float) b;
                case short s:
                    return (float) s;
                case long l:
                    return (float) l;
                default:
                    throw new InvalidCastException();
            }
        }
    }
}