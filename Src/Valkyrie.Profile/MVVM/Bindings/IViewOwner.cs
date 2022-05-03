using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public interface IViewOwner
    {
        GameObject View { set; }
    }
}