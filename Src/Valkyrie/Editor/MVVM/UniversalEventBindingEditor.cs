using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine.Events;
using Utils;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(EventBinding), true)]
    public class UniversalEventBindingEditor : AbstractBindingEditor
    {
        private SerializedProperty _eventName;
        private SerializedProperty _eventCallback;
        
        private EventBinding Component => (EventBinding) serializedObject.targetObject;

        private void OnEnable()
        {
            _eventName = serializedObject.FindProperty(nameof(_eventName));
            _eventCallback = serializedObject.FindProperty(nameof(_eventCallback));
        }

        protected override void DrawGui()
        {
            _eventName.stringValue =
                EditorUtils.DrawPopup("Event",
                    _eventName.stringValue,
                    FindEvents());

            _eventCallback.stringValue =
                EditorUtils.DrawPopup("Event call", 
                    _eventCallback.stringValue, 
                    FindCalls());
        }

        private List<string> FindCalls()
        {
            var callList = new List<string>();
            foreach (var type in AllAvailableViewModels(Component.gameObject))
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(u => u.GetCustomAttribute<BindingAttribute>() != null && u.GetParameters().Length == 0)
                    .OrderBy(u => u.Name).Select(u => $"{type.FullName}/{u.Name}"/*.Replace(".", "/")*/);
                callList.AddRange(methods);
            }

            return callList;
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
    }
}