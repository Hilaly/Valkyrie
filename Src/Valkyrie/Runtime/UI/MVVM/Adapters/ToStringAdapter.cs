using System;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class ToStringAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return true;
        }

        public Type GetResultType()
        {
            return typeof(string);
        }

        public object Convert(object source)
        {
            if (source == null)
                return string.Empty;
            return source.ToString();
        }
    }
}