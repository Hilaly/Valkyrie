using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace Valkyrie.MVVM.Bindings
{
    public class EventBinding : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private string _eventName;
        [SerializeField] private string _eventCallback;
#pragma warning restore 649

        void Start()
        {
            Bind();
        }

        void Bind()
        {
            AbstractBindingComponent.SplitTypeProperty(_eventName, out var componentType, out var componentEventName);
            AbstractBindingComponent.SplitTypeProperty(_eventCallback, out var viewModelType, out var viewModelMethod);
            
            var component = gameObject.GetComponent(componentType);
            // ReSharper disable once PossibleNullReferenceException
            var componentEvent = (UnityEvent) component.GetType().GetProperty(componentEventName).GetValue(component);

            var model = AbstractBindingComponent.GetModel(gameObject, viewModelType, out var disposeHandler);
            
            var methodInfo = model.GetType().GetMethod(viewModelMethod, BindingFlags.Instance | BindingFlags.Public);
            if (methodInfo == null)
            {
                Debug.LogErrorFormat("Couldn't find method {0} at ViewModel {1}", viewModelMethod,
                    model.GetType().Name);
                return;
            }

            componentEvent.Subscribe(() => methodInfo.Invoke(model, null)).AttachTo(gameObject);
        }
    }
}