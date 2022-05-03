using System;

namespace Valkyrie.MVVM.Adapters
{
    public class LongToBigNumberStringAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return type == typeof(long);
        }

        public Type GetResultType()
        {
            return typeof(string);
        }

        public object Convert(object source)
        {
            return ((long) source).ToBigNumberString();
        }
    }
}