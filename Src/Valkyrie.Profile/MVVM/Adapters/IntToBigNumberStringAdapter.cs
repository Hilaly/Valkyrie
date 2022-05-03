using System;

namespace Valkyrie.MVVM.Adapters
{
    public class IntToBigNumberStringAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type) => type == typeof(int);

        public Type GetResultType() => typeof(string);

        public object Convert(object source) => ((int) source).ToBigNumberString();
    }
}