using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valkyrie.Di;
using Valkyrie.Tools;

namespace Valkyrie.UserInput.UnitySpecific
{
    [RequireComponent(typeof(Button))]
    public class UiVirtualButton : MonoBehaviour, IVirtualButton, IPointerDownHandler, IPointerUpHandler
    {
#pragma warning disable 649
        [Inject] private IControls _controls;
        [SerializeField] private string _buttonKey;
#pragma warning restore 649
        
        public string ButtonName
        {
            get => _buttonKey;
            set
            {
                if (value == _buttonKey)
                    return;
                
                _buttonKey = value;
                Subscribe();
            }
        }

        private IDisposable _subscription;
        private bool _isPressed;

        private void Awake()
        {
            Subscribe();
        }

        void Subscribe()
        {
            _subscription?.Dispose();
            if(_buttonKey.NotNullOrEmpty() && _controls != null)
                _subscription = _controls.RegisterButton(_buttonKey, this);
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        public bool IsPressed()
        {
            return _isPressed;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
        }
    }
}