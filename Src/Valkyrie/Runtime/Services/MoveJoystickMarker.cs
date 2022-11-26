using System;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.UserInput.UnitySpecific;

namespace Services
{
    [RequireComponent(typeof(Joystick2Axis))]
    public class MoveJoystickMarker : MonoBehaviour
    {
        [Inject] private InputService _inputService;
        private IDisposable _onDestroy;
        
        private void Awake()
        {
            _onDestroy = _inputService.RegisterMoveJoystick(GetComponent<Joystick2Axis>());
        }

        private void OnDestroy()
        {
            _onDestroy?.Dispose();
            _onDestroy = null;
        }
    }
}