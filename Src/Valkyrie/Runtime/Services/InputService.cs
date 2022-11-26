using System;
using System.Collections.Generic;
using Valkyrie.Di;
using Valkyrie.UserInput;
using Valkyrie.UserInput.UnitySpecific;

namespace Services
{
    public interface IInputService
    {
        public IVirtualJoystick MoveInput { get; }
        public IReadOnlyList<IVirtualJoystick> Buttons { get; }
    }

    class InputService : IInputService
    {
        private readonly ComplexJoystick _moveInput;
        private readonly List<ComplexJoystick> _buttons = new();

        public IVirtualJoystick MoveInput => _moveInput;

        public IReadOnlyList<IVirtualJoystick> Buttons => _buttons;

        public InputService()
        {
            _moveInput = new ComplexJoystick();
#if UNITY_EDITOR || UNITY_STANDALONE
            _moveInput.Add(new VirtualAxisJoystick(
                new UnityInputAxis("Horizontal"),
                new UnityInputAxis("Vertical")));
#endif
        }

        public IDisposable RegisterMoveJoystick(IVirtualJoystick joystick)
        {
            _moveInput.Add(joystick);
            return new ActionDisposable(() => _moveInput.Remove(joystick));
        }

        public IDisposable RegisterButton(IVirtualJoystick joystick, int index)
        {
            while (_buttons.Count <= index)
                _buttons.Add(new ComplexJoystick());
            var button = _buttons[index];
            button.Add(joystick);
            return new ActionDisposable(() => button.Remove(joystick));
        }
    }
}