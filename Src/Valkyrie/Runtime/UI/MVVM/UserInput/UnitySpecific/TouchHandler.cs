using UnityEngine;
using UnityEngine.EventSystems;
using Valkyrie.UnityExtensions.Components;

namespace Valkyrie.UserInput.UnitySpecific
{
    [RequireComponent(typeof(NullGraphics))]
    public class TouchHandler : MonoBehaviour
        , IDragHandler, IBeginDragHandler, IEndDragHandler
        , IVirtualJoystick
    {
        private bool _dragged;
        [SerializeField] private AnimationCurve _sensitivityCurve;
        [SerializeField] private UnityEngine.Vector2 _sensitivity = new UnityEngine.Vector2(400, 100);

        public enum Mode
        {
            Normalized,
            Balanced,
            Evaluated
        }

        private float _xPrev;
        private float _yPrev;
        
#pragma warning disable 649
        [SerializeField] private Mode _mode;
#pragma warning restore 649
        
        public void OnDrag(PointerEventData eventData)
        {
            switch (_mode)
            {
                case Mode.Normalized:
                    Value = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height);
                    break;
                case Mode.Balanced:
                {
                    var temp = Mathf.Min(Screen.width, Screen.height);
                    Value = new Vector2(eventData.delta.x / temp * ((float)Screen.width / Screen.height), eventData.delta.y / temp);
                    break;
                }
                case Mode.Evaluated:
                {
                    var x = eventData.delta.x; // / Screen.dpi;
                    var y = eventData.delta.y; // / Screen.dpi;
                    var normX = eventData.delta.x / Screen.dpi;
                    var normY = eventData.delta.y / Screen.dpi;
                    var inputL = Mathf.Sqrt(normX * normX + normY * normY);
                    var mult = _sensitivityCurve.Evaluate(inputL / Time.deltaTime);
                    var newX = mult * x * _sensitivity.x;
                    var newY = mult * y * _sensitivity.y;
                    Value = new Vector2(
                        (newX + _xPrev) * 0.5f,
                        (newY + _yPrev) * 0.5f
                    );
                    _xPrev = newX;
                    _yPrev = newY;
                    break;
                }
            }

            _dragged = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            switch (_mode)
            {
                case Mode.Normalized:
                    Value = new Vector2(eventData.delta.x / Screen.width, eventData.delta.y / Screen.height);
                    break;
                case Mode.Balanced:
                {
                    var temp = Mathf.Min(Screen.width, Screen.height);
                    Value = new Vector2(eventData.delta.x / temp * ((float)Screen.width / Screen.height), eventData.delta.y / temp);
                    break;
                }
                case Mode.Evaluated:
                {
                    var x = eventData.delta.x; // / Screen.dpi;
                    var y = eventData.delta.y; // / Screen.dpi;
                    var normX = eventData.delta.x / Screen.dpi;
                    var normY = eventData.delta.y / Screen.dpi;
                    var inputL = Mathf.Sqrt(normX * normX + normY * normY);
                    var mult = _sensitivityCurve.Evaluate(inputL / Time.deltaTime);
                    var newX = mult * x * _sensitivity.x;
                    var newY = mult * y * _sensitivity.y;
                    Value = new Vector2(
                        (newX + _xPrev) * 0.5f,
                        (newY + _yPrev) * 0.5f
                    );
                    _xPrev = newX;
                    _yPrev = newY;
                    break;
                }
            }

            _dragged = true;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _xPrev = _yPrev = 0;
            Value = Vector2.zero;
        }
        

        public void Update()
        {
            if (!_dragged)
                Value = Vector2.zero;
            _dragged = false;
        }

        public Vector2 Value { get; private set; }
    }
}