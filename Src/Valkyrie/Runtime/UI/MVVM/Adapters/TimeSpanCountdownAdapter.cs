using System;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class TimeSpanCountdownAdapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return type == typeof(TimeSpan);
        }

        public Type GetResultType()
        {
            return typeof(string);
        }

        public object Convert(object source)
        {
            var timeSpan = (TimeSpan) source;

            return timeSpan.TotalSeconds.ToString("F1");
        }
    }
}