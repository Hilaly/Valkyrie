using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Valkyrie.Di;
using Valkyrie.Tools;

namespace Valkyrie.UserInput.UnitySpecific
{
    public class LayeredInputModule : StandaloneInputModule
    {
#pragma warning disable 649
        [Inject] private IInput _helper;
#pragma warning restore 649
        
        private readonly List<RaycastResult> _cachedRaycastResults = new List<RaycastResult>();

        class DuplicatedTouch
        {
            private readonly int _sourceId;

            public int SourceId => _sourceId;
            public Touch Target;

            public DuplicatedTouch(Touch source, int id)
            {
                _sourceId = source.fingerId;
                Target = source.MakeCopy();
                Target.fingerId = id;
            }

            public void Update(List<Touch> touch)
            {
                var source = touch.Find(u => u.fingerId == _sourceId);
                Target.position = source.position;
                Target.deltaPosition = source.deltaPosition;
                Target.phase = source.phase;
            }
        }

        readonly List<DuplicatedTouch> _duplicates = new List<DuplicatedTouch>();

        protected override void Awake()
        {
            ProjectContext.Instance.Container.Inject(this);
            base.Awake();
        }

        public override void Process()
        {
            bool selectedObject = SendUpdateEventToSelectedObject();
            if (eventSystem.sendNavigationEvents)
            {
                if (!selectedObject)
                    selectedObject |= SendMoveEventToSelectedObject();
                if (!selectedObject)
                    SendSubmitEventToSelectedObject();
            }

            var touches = _helper.GetTouches();
            foreach (var duplicatedTouch in _duplicates)
            {
                duplicatedTouch.Update(touches);
            }
            touches.AddRange(_duplicates.Select(u => u.Target));
            
            foreach (var touch in touches)
            {
                bool pressed;
                bool released;
                var pointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
                
                _cachedRaycastResults.Clear();
                eventSystem.RaycastAll(pointerEventData, _cachedRaycastResults);
                var adds = 10000;

                if (touch.phase == TouchPhase.Began)
                {
                    foreach (var result in GetLayered(_cachedRaycastResults, pointerEventData.pointerCurrentRaycast))
                    {
                        if (result.gameObject == pointerEventData.pointerCurrentRaycast.gameObject)
                        {
                            Process(pointerEventData, pressed, released);
                        }
                        else
                        {
                            bool tempPressed;
                            bool tempReleased;
                            var d = new DuplicatedTouch(touch, touch.fingerId + adds);
                            _duplicates.Add(d);
                            adds += 10000;
                            var tempPointerEventData =
                                GetTouchPointerEventData(d.Target, out tempPressed, out tempReleased);
                            tempPointerEventData.pointerCurrentRaycast = result;

                            Process(tempPointerEventData, tempPressed, tempReleased);
                        }
                    }
                }
                else
                {
                    Process(pointerEventData, pressed, released);
                }
            }

            _duplicates.RemoveAll(u => u.Target.phase == TouchPhase.Ended || u.Target.phase == TouchPhase.Canceled);
        }

        void Process(PointerEventData data, bool pressed, bool released)
        {
            ProcessTouchPress(data, pressed, released);
            if (!released)
            {
                ProcessMove(data);
                ProcessDrag(data);
            }
            else
                RemovePointerData(data);
        }

        RaycastResult[] GetLayered(List<RaycastResult> list, RaycastResult def)
        {
            //return list.ToArray();
            if (list.Count == 0)
                return new[] {def};
            var layers = new HashSet<int>();
            for (var i = 0; i < list.Count;)
            {
                var ol = list[i].gameObject.layer;
                if(layers.Contains(ol))
                    list.RemoveAt(i);
                else
                {
                    layers.Add(ol);
                    ++i;
                }
            }

            return list.ToArray();
        }
    }
}