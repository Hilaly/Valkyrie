using System;
using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class TimeSpanToStringAdapter : IBindingAdapter
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
            
            var d = Mathf.FloorToInt((float) timeSpan.TotalDays);
            if (d > 0)
                return $"{d}d : {timeSpan.Hours:D2}h";
            
            var h = Mathf.FloorToInt((float) timeSpan.TotalHours);
            if (h > 0)
                return $"{h:D2}h : {timeSpan.Minutes:D2}m";
            return $"{Mathf.FloorToInt((float) timeSpan.TotalMinutes):D2}m : {timeSpan.Seconds:D2}s";
        }
    }
}