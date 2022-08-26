using UnityEngine;

namespace Valkyrie.UserInput
{
    public class VirtualAxisJoystick : IVirtualJoystick
    {
        private readonly IVirtualAxis _horizontal;
        private readonly IVirtualAxis _vertical;

        public bool IsPressed => Value.sqrMagnitude > 0f;
        public Vector2 Value => new Vector2(_horizontal.Value, _vertical.Value);

        public VirtualAxisJoystick(IVirtualAxis horizontal, IVirtualAxis vertical)
        {
            _horizontal = horizontal;
            _vertical = vertical;
        }
    }
}