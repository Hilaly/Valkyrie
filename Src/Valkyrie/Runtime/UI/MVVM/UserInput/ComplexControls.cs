using System;
using System.Collections.Generic;
using Valkyrie.Di;

namespace Valkyrie.UserInput
{
    class ComplexControls : IControls
    {
        readonly Dictionary<string, ComplexAxis> _axises = new Dictionary<string, ComplexAxis>();
        private readonly Dictionary<string, ComplexButton> _buttons = new Dictionary<string, ComplexButton>();
        readonly Dictionary<string, ComplexJoystick> _joysticks = new Dictionary<string, ComplexJoystick>();

        ComplexAxis GetComplexAxis(string id)
        {
            ComplexAxis result;
            if (!_axises.TryGetValue(id, out result))
            {
                result = new ComplexAxis();
                _axises.Add(id, result);
            }

            return result;
        }
        
        ComplexButton GetComplexButton(string id)
        {
            ComplexButton result;
            if (!_buttons.TryGetValue(id, out result))
            {
                result = new ComplexButton();
                _buttons.Add(id, result);
            }

            return result;
        }
        
        ComplexJoystick GetComplexJoystick(string id)
        {
            ComplexJoystick result;
            if (!_joysticks.TryGetValue(id, out result))
            {
                result = new ComplexJoystick();
                _joysticks.Add(id, result);
            }

            return result;
        }
        
        public IVirtualAxis GetAxis(string id)
        {
            return GetComplexAxis(id);
        }

        public IVirtualButton GetButton(string id)
        {
            return GetComplexButton(id);
        }

        public IVirtualJoystick GetJoystick(string id)
        {
            return GetComplexJoystick(id);
        }

        public IDisposable RegisterAxis(string id, IVirtualAxis axis)
        {
            var c = GetComplexAxis(id);
            c.Add(axis);
            return new ActionDisposable(() => c.Remove(axis));
        }

        public IDisposable RegisterButton(string id, IVirtualButton button)
        {
            var c = GetComplexButton(id);
            c.Add(button);
            return new ActionDisposable(() => c.Remove(button));
        }

        public IDisposable RegisterJoystick(string id, IVirtualJoystick joystick)
        {
            var c = GetComplexJoystick(id);
            c.Add(joystick);
            return new ActionDisposable(() => c.Remove(joystick));
        }
    }
}