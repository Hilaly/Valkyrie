using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using Utils;
using Valkyrie.MVVM.Bindings;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(FieldBinding), true)]
    public class FieldBindingEditor : AbstractBindingEditor
    {
        private SerializedProperty _viewModelProperty;
        
        private FieldBinding Component => (FieldBinding) serializedObject.targetObject;

        private void OnEnable()
        {
            _viewModelProperty = serializedObject.FindProperty(nameof(_viewModelProperty));
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
            EditorGUILayout.LabelField("TYPE: " + (_viewModelProperty.stringValue.NotNullOrEmpty()
                ? _viewModelProperty.stringValue.Split(':')[1]
                : "None"));
        }
    }
}