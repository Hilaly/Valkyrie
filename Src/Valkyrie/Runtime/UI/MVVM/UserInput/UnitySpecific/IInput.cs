using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.UserInput.UnitySpecific
{
    public interface IInput
    {
        List<Touch> GetTouches();
    }
}