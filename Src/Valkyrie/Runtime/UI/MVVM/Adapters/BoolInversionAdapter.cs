using System;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class BoolInversionAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return type == typeof(bool);
        }

        public Type GetResultType()
        {
            return typeof(bool);
        }

        public object Convert(object source)
        {
            var temp = (bool) source;
            return !temp;
        }
    }
}