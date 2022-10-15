using System.Linq;
using UnityEngine;

namespace Valkyrie.UserInput
{
   public class ComplexJoystick : GenericInnerListOwner<IVirtualJoystick>, IVirtualJoystick
   {
       public bool IsPressed => Values.Any(x => x.IsPressed);

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