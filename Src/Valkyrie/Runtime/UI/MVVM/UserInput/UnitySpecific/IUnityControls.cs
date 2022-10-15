using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.UserInput.UnitySpecific
{
    public interface IUnityControls : IControls
    {
        List<Touch> GetTouches();
    }
}