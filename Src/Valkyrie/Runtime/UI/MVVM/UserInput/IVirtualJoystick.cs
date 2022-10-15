using UnityEngine;

namespace Valkyrie.UserInput
{
    public interface IVirtualJoystick
    {
        bool IsPressed { get; }
        Vector2 Value { get; }
    }
}