using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Utils;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(TemplateSelector), true)]
    public class TemplateSelectorEditor : AbstractBindingEditor
    {
        private SerializedProperty _viewModelProperty;
        private SerializedProperty _templateSelector;
        private SerializedProperty injectNeeded;
        
        private TemplateSelector Component => (TemplateSelector) serializedObject.targetObject;

        private void OnEnable()
        {
            _viewModelProperty = serializedObject.FindProperty(nameof(_viewModelProperty));
            _templateSelector = serializedObject.FindProperty(nameof(_templateSelector));
            injectNeeded = serializedObject.FindProperty(nameof(injectNeeded));
        }

        protected override void DrawGui()
        {
            //ViewModel property
            var viewModelProperties = new List<string>();
            foreach (var property in FindAllProperties(AllAvailableViewModels(Component.gameObject),
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                info => info.GetCustomAttribute<BindingAttribute>() != null &&
                        Component.IsValidViewModelProperty(info)))
                viewModelProperties.Add(property);

            _viewModelProperty.stringValue =
                EditorUtils.DrawPopup("Property",
                    _viewModelProperty.stringValue,
                    viewModelProperties);

            EditorGUILayout.PropertyField(_templateSelector, new GUIContent("Templates root"));
        }
    }
}