using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.UserInput.UnitySpecific
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class InputHelper : IInput
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        class TouchCreator
        {
            private Touch _touch;

            public float deltaTime
            {
                get => _touch.deltaTime;
                set => _touch.deltaTime = value;
            }

            public int tapCount
            {
                get => _touch.tapCount;
                set => _touch.tapCount = value;
            }

            public TouchPhase phase
            {
                get => _touch.phase;
                set => _touch.phase = value;
            }

            public Vector2 deltaPosition
            {
                get => _touch.deltaPosition;
                set => _touch.deltaPosition = value;
            }

            public int fingerId
            {
                get => _touch.fingerId;
                set => _touch.fingerId = value;
            }

            public Vector2 position
            {
                get => _touch.position;
                set => _touch.position = value;
            }

            public Vector2 rawPosition
            {
                get => _touch.rawPosition;
                set => _touch.rawPosition = value;
            }

            public Touch Create()
            {
                return _touch;
            }

            public TouchCreator()
            {
                _touch = new Touch();
            }
        }

        private TouchCreator _leftTouch;
        private TouchCreator _rightTouch;
        private TouchCreator _middleTouch;

        private TouchCreator GetForButton(int button, TouchCreator result)
        {
            if (result == null)
                result = new TouchCreator();

            var mousePosition = UnityEngine.Input.mousePosition;
            var newPosition = new Vector2(mousePosition.x, mousePosition.y);

            result.fingerId = button;

            if (UnityEngine.Input.GetMouseButtonDown(button))
            {
                result.phase = TouchPhase.Began;
                result.deltaPosition = new Vector2(0, 0);
                result.position = newPosition;
            }
            else if (UnityEngine.Input.GetMouseButtonUp(button))
            {
                result.phase = TouchPhase.Ended;
                result.deltaPosition = newPosition - result.position;
                result.position = newPosition;
            }
            else if (UnityEngine.Input.GetMouseButton(button))
            {
                result.deltaPosition = newPosition - result.position;
                result.phase = Mathf.Abs(result.deltaPosition.magnitude) < Mathf.Epsilon
                    ? TouchPhase.Stationary
                    : TouchPhase.Moved;

                result.position = newPosition;
            }
            else
            {
                result = null;
            }

            return result;
        }
#endif
        public List<Touch> GetTouches()
        {
            var touches = new List<Touch>(UnityEngine.Input.touches);

#if UNITY_EDITOR || UNITY_STANDALONE
            _leftTouch = GetForButton(0, _leftTouch);
            _rightTouch = GetForButton(1, _rightTouch);
            _middleTouch = GetForButton(2, _middleTouch);

            if (_leftTouch != null)
                touches.Add(_leftTouch.Create());
            if (_rightTouch != null)
                touches.Add(_rightTouch.Create());
            if (_middleTouch != null)
                touches.Add(_middleTouch.Create());
#endif

            return touches;
        }
    }
}
