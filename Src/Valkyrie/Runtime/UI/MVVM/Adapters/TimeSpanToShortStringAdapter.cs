using System;
using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class TimeSpanToShortStringAdapter : IBindingAdapter
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
                return $"{d}:{timeSpan.Hours:D2}";
            
            var h = Mathf.FloorToInt((float) timeSpan.TotalHours);
            if (h > 0)
                return $"{h:D2}:{timeSpan.Minutes:D2}";
            return $"{Mathf.FloorToInt((float) timeSpan.TotalMinutes):D2}:{timeSpan.Seconds:D2}";
        }
    }
}