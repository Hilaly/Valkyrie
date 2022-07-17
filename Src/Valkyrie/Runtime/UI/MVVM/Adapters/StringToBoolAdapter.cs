using System;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Adapters
{
    public class StringToBoolAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return typeof(string).IsAssignableFrom(type);
        }

        public Type GetResultType()
        {
            return typeof(bool);
        }

        public object Convert(object source) => ((string) source).NotNullOrEmpty();
    }
}