using System;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class LongToStringAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return typeof(long).IsAssignableFrom(type);
        }

        public Type GetResultType()
        {
            return typeof(string);
        }

        public object Convert(object source)
        {
            return ((long) source).ToString();
        }
    }
}