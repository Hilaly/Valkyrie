using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public class EventFlowBinding : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] private string _eventName;
        [SerializeField] private string _eventCallback;
        [SerializeField] List<string> _argNames = new List<string>();
        [SerializeField] List<string> _viewModelProperties = new List<string>();
#pragma warning restore 649

        /*
        private IApplicationRouter _router;
        
        [Inject] void Init(IApplicationRouter router)
        {
            _router = router;
            
            Bind();
        }

        void Bind()
        {
            AbstractBindingComponent.SplitTypeProperty(_eventName, out var componentType, out var componentEventName);

            var getters = new List<Func<KeyValuePair<string, object>>>();
            for (var i = 0; i < _viewModelProperties.Count; ++i)
            {
                var i1 = i;
                var keyName = _argNames[i1].Split(' ')[1];
                if (AbstractBindingComponent.SplitTypeProperty(_viewModelProperties[i1], out var typeName,
                    out var propertyName))
                {
                    var viewModel = AbstractBindingComponent.GetModel(gameObject, typeName, out var disposeHandler);
                    var property = viewModel.GetType().GetProperty(propertyName);
                    getters.Add(() => new KeyValuePair<string, object>(keyName, property.GetValue(viewModel, null)));
                }
                else
                {
                    getters.Add(() => new KeyValuePair<string, object>(keyName,
                        _viewModelProperties[i1].NotNullOrEmpty() ? _viewModelProperties[i1] : null));
                }
            }
            
            var component = gameObject.GetComponent(componentType);
            // ReSharper disable once PossibleNullReferenceException
            var componentEvent = (UnityEvent) component.GetType().GetProperty(componentEventName).GetValue(component);

            componentEvent.AddListener(delegate
            {
                var tempStr = _eventCallback;
                if (tempStr.IndexOf("(") >= 0)
                {
                    tempStr = tempStr.Substring(0, tempStr.IndexOf("("));
                }
                
                var pathInfo = new RouteInfo(tempStr);
                foreach (var func in getters)
                {
                    var arg = func();
                    if (arg.Value == null)
                        continue;
                    pathInfo.Args.Add(arg.Key, arg.Value.ToString());
                }

                _router.RouteTo(pathInfo.ToString());
            });
        }
        */
    }
}