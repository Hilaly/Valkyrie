using UnityEngine;

namespace Valkyrie.UserInput
{
    class ComplexJoystick : GenericInnerListOwner<IVirtualJoystick>, IVirtualJoystick
    {
        public Vector2 Value
        {
            get
            {
                Vector2 result = Vector2.zero;
                foreach (var joystick in Values)
                {
                    result += joystick.Value;
                }

                return result;
            }
        }
    }
}