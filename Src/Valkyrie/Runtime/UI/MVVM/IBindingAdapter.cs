using System;

namespace Valkyrie.MVVM
{
    public interface IBindingAdapter
    {
        bool IsAvailableSourceType(Type type);
        Type GetResultType();
        
        object Convert(object source);
    }
}