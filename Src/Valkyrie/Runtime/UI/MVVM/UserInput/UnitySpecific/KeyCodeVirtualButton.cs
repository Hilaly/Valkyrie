using UnityEngine;

namespace Valkyrie.UserInput.UnitySpecific
{
    public class KeyCodeVirtualButton : IVirtualButton
    {
        private readonly KeyCode _keyCode;

        public KeyCodeVirtualButton(KeyCode keyCode)
        {
            _keyCode = keyCode;
        }

        public bool IsPressed()
        {
            return UnityEngine.Input.GetKey(_keyCode);
        }
    }
}