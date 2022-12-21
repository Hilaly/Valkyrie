using System;

namespace Valkyrie.UserInput
{
    public interface IControls
    {
        IVirtualAxis GetAxis(string id);
        IVirtualButton GetButton(string id);
        IVirtualJoystick GetJoystick(string id);

        IDisposable RegisterAxis(string id, IVirtualAxis axis);
        IDisposable RegisterButton(string id, IVirtualButton button);
        IDisposable RegisterJoystick(string id, IVirtualJoystick joystick);
    }
}