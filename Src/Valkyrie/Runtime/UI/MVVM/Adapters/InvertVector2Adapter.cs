using System;
using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Adapters
{
    public class InvertVector2Adapter : IBindingAdapter
    {
        public bool IsAvailableSourceType(Type type)
        {
            return typeof(UnityEngine.Vector2).IsAssignableFrom(type);
        }

        public Type GetResultType()
        {
            return typeof(UnityEngine.Vector2);
        }

        public object Convert(object source)
        {
            return ((Vector2) source) * -1f;
        }
    }
}