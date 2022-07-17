using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valkyrie.MVVM;

namespace Valkyrie.UserInput.UnitySpecific
{
    [Binding] public class Joystick2Axis : MonoBehaviour, IMoveJoystick
        , IDragHandler, IBeginDragHandler, IEndDragHandler
        , IPointerDownHandler, IPointerUpHandler
    {
#pragma warning disable 0649
        // ReSharper disable InconsistentNaming
        [SerializeField] private RectTransform _viewTransform;
        [SerializeField] private RectTransform _moveLeft;
        [SerializeField] private RectTransform _moveRight;
        [SerializeField] private RectTransform _moveUp;
        [SerializeField] private RectTransform _moveDown;
        [SerializeField] private RectTransform _stick;
        [SerializeField] private AnimationCurve _sensitivity;
        [SerializeField] private bool _isRound;
        [Range(0, 0.5f)] [SerializeField] private float _axisHardDeadZone = 0.25f;
        // ReSharper restore InconsistentNaming
#pragma warning restore 0649
        
        private Vector2 _defaultPosition;
        private bool _cruiseControl;
        [SerializeField] private bool _isDynamic;

        public Vector2 Value { get; private set; } = Vector2.zero;

        [Binding] public bool IsDynamic
        {
            get => _isDynamic;
            set => _isDynamic = value;
        }

        [Binding] public bool IsPressed { get; private set; }

        public bool CruiseControl
        {
            get => _cruiseControl;
            set
            {
                if(_cruiseControl == value)
                    return;
                _cruiseControl = value;
                if(!_cruiseControl)
                    Reset();
            }
        }

        #region DiComponent

        private void Awake()
        {
            _defaultPosition = _viewTransform.anchoredPosition;
        }

        private void OnDisable()
        {
            Reset();
        }
        
        #endregion

        private void Reset()
        {
            IsPressed = false;

            Value = Vector2.zero;
            _stick.anchoredPosition = Vector2.zero;
        }

        #region UnityHandlers
        
        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            
            if(IsDynamic)
                UpdateDynamicView(eventData.position);
            else
                OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;

            if (IsDynamic)
                ResetDynamicView();
			
            if (!_cruiseControl)
                Reset();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsPressed = true;

            if(!IsDynamic)
                return;
			
            OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsPressed = false;

            if(!IsDynamic)
                return;

            ResetDynamicView();
			
            if (!_cruiseControl)
                Reset();
        }

        public void OnDrag(PointerEventData eventData)
        {
            IsPressed = true;

            var downPosition = _moveDown.position;
            var leftPosition = _moveLeft.position;
            var yAxisDelta = _moveUp.position - downPosition;
            var xAxisDelta = _moveRight.position - leftPosition;
            var pointerVecY = eventData.position - new Vector2(downPosition.x, downPosition.y);
            var pointerVecX = eventData.position - new Vector2(leftPosition.x, leftPosition.y);

            var yAxis = Mathf.Clamp(
                (Vector2.Dot(yAxisDelta, pointerVecY) / yAxisDelta.magnitude - yAxisDelta.magnitude * 0.5f) 
                / (yAxisDelta.magnitude * 0.5f), -1.0f, 1.0f);
			
            var xAxis = Mathf.Clamp(
                (Vector2.Dot(xAxisDelta, pointerVecX) / xAxisDelta.magnitude - xAxisDelta.magnitude * 0.5f) 
                / (xAxisDelta.magnitude * 0.5f), -1.0f, 1.0f);

            var vectorResult = new Vector2(xAxis, yAxis);
            if (vectorResult.sqrMagnitude < _axisHardDeadZone * _axisHardDeadZone)
            {
                vectorResult.x = ApplyDeadZone(vectorResult.x);
                vectorResult.y = ApplyDeadZone(vectorResult.y);
            }
			
            if (_isRound && vectorResult.sqrMagnitude > 1)
            {
                vectorResult.Normalize();
            }

            var length = vectorResult.magnitude;
            var multiplier = _sensitivity.Evaluate(length);
            Value = (vectorResult.normalized * multiplier);
			
            _stick.position = new Vector3(
                _moveLeft.position.x + (xAxisDelta.x + vectorResult.x * xAxisDelta.magnitude) * 0.5f, 
                _moveDown.position.y + (yAxisDelta.y + vectorResult.y * yAxisDelta.magnitude) * 0.5f, 0);
        }

        #endregion

        #region DeadZone
        
        private float ApplyDeadZone(float val)
        {
            if (Mathf.Abs(val) < _axisHardDeadZone)
            {
                return 0;
            }
			
            if (val > 0)
            {
                return (val - _axisHardDeadZone) / (1 - _axisHardDeadZone);
            }
			
            if (val < 0)
            {
                return (val + _axisHardDeadZone) / (1 - _axisHardDeadZone);
            }

            return val;
        }
        
        #endregion

        #region Dynamic Joystick
        
        void ResetDynamicView()
        {
            IsPressed = false;

            _viewTransform.anchoredPosition = _defaultPosition;
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        }
        void UpdateDynamicView(Vector2 point)
        {
            _viewTransform.localPosition = new Vector3((point.x /*- Screen.width / 2.0f*/) / _viewTransform.parent.lossyScale.x, 
                (point.y /*- Screen.height / 2.0f*/) / _viewTransform.parent.lossyScale.y, 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate(gameObject.GetComponent<RectTransform>());
        }
        
        #endregion
    }
}