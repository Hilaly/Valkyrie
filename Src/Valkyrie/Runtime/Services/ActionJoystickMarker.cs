using System;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.UserInput.UnitySpecific;

namespace Services
{
    [RequireComponent(typeof(Joystick2Axis))]
    public class ActionJoystickMarker : MonoBehaviour
    {
        [Inject] private InputService _inputService;

        [SerializeField, Range(0, 10)] private int buttonIndex;

        private IDisposable _onDestroy;

        private void Awake()
        {
            _onDestroy = _inputService.RegisterButton(GetComponent<Joystick2Axis>(), buttonIndex);
        }

        private void OnDestroy()
        {
            _onDestroy?.Dispose();
            _onDestroy = null;
        }
    }
}