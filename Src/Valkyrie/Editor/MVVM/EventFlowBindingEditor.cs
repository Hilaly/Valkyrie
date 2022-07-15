using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Valkyrie.MVVM.Bindings;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(EventFlowBinding))]
    public class EventFlowBindingEditor : AbstractBindingEditor
    {
        private EventFlowBinding Component => (EventFlowBinding) serializedObject.targetObject;
        
        private SerializedProperty _eventName;
        private SerializedProperty _eventCallback;
        private SerializedProperty _argNames;
        private SerializedProperty _viewModelProperties;
        
        private List<string> _routes;

        private void OnEnable()
        {
            _eventName = serializedObject.FindProperty(nameof(_eventName));
            _eventCallback = serializedObject.FindProperty(nameof(_eventCallback));
            _argNames = serializedObject.FindProperty(nameof(_argNames));
            _viewModelProperties = serializedObject.FindProperty(nameof(_viewModelProperties));
        }


        protected override void DrawGui()
        {
            _eventName.stringValue =
                EditorUtils.DrawPopup("Event",
                    _eventName.stringValue,
                    FindEvents());
            
            var route = EditorUtils.DrawPopup("Route",
                _eventCallback.stringValue,
                FindAllRoutes()
            );
            if (route.NotNullOrEmpty() && _eventCallback.stringValue != route)
            {
                var start = route.IndexOf("(", StringComparison.Ordinal) + 1;
                var end = route.IndexOf(")", StringComparison.Ordinal);
                var argsList = route.Substring(start, end - start);
                var names = argsList.Split(",", StringSplitOptions.RemoveEmptyEntries);

                _viewModelProperties.arraySize = names.Length;
                _argNames.arraySize = names.Length;
                for (var i = 0; i < names.Length; ++i)
                {
                    _argNames.GetArrayElementAtIndex(i).stringValue = names[i];
                    _viewModelProperties.GetArrayElementAtIndex(i).stringValue = null;
                }
            }
            _eventCallback.stringValue = route;
            
            for (var i = 0; i < _viewModelProperties.arraySize; ++i)
            {
                EditorGUILayout.PropertyField(_argNames.GetArrayElementAtIndex(i), new GUIContent("Name"));
                
                var prop = _viewModelProperties.GetArrayElementAtIndex(i);
                //ViewModel property
                var tempStr = EditorUtils.DrawPopup("Model property",
                    prop.stringValue,
                    FindAllViewModelProperties(Component.gameObject));
                if(tempStr.NotNullOrEmpty() && tempStr != prop.stringValue)
                    prop.stringValue = tempStr;
                tempStr = EditorGUILayout.TextField("Value", prop.stringValue);
                if(tempStr.NotNullOrEmpty() && tempStr != prop.stringValue)
                    prop.stringValue = tempStr;
            }

            if (GUILayout.Button("Add property"))
            {
                _argNames.arraySize += 1;
                _viewModelProperties.arraySize += 1;
            }

            if (_viewModelProperties.arraySize > 0 && GUILayout.Button("Remove last property"))
            {
                _argNames.arraySize -= 1;
                _viewModelProperties.arraySize -= 1;
            }
            
            EditorGUILayout.Separator();

            if (GUILayout.Button("Reload keys"))
            {
                _routes = null;
            }
        }

        private List<string> FindEvents()
        {
            var eventsList = new List<string>();
            foreach (var component in AllAvailableViews(Component))
            {
                var type = component.GetType();
                var properties = type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty)
                    .Where(u => typeof(UnityEvent).IsAssignableFrom(u.PropertyType)).OrderBy(u => u.Name)
                    .Select(u => $"{type.FullName}/{u.Name}"/*.Replace(".", "/")*/);
                eventsList.AddRange(properties);
            }

            return eventsList;
        }
        
        List<string> FindAllRoutes()
        {
            if (_routes == null)
            {
                /*TODO _routes = typeof(BaseController).GetAllSubTypes(t => !t.IsAbstract)
                    .SelectMany(type =>
                    {
                        var methods = Flow.Utils.CollectUIActionMethods(type);
                        return methods.Select(m => $"{type.Name}/{m}");
                    })
                    .ToList();*/
                _routes = new List<string>();
            }

            return _routes;
        }

        List<string> FindAllViewModelProperties(GameObject o)
        {
            var flags = BindingFlags.GetProperty;

            bool ViewModelPropertiesFilter(PropertyInfo info) =>
                info.GetCustomAttribute<BindingAttribute>() != null;

            return FindAllProperties(AllAvailableViewModels(o), flags, ViewModelPropertiesFilter);
        }

    }
}