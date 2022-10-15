using System;

namespace Utils
{
    public interface IBindingAdapter
    {
        bool IsAvailableSourceType(Type type);
        Type GetResultType();
        
        object Convert(object source);
    }
}