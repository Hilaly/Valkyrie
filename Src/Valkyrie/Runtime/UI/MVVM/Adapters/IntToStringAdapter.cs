using System;

namespace Valkyrie.MVVM.Adapters
{
    public class IntToStringAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return typeof(int).IsAssignableFrom(type);
        }

        public Type GetResultType()
        {
            return typeof(string);
        }

        public object Convert(object source)
        {
            return ((int) source).ToString();
        }
    }
}