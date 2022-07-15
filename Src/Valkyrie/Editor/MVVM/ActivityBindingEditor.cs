using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Valkyrie.MVVM.Bindings;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(ActivityBinding), true)]
    public class ActivityBindingEditor : AbstractBindingEditor
    {
        private SerializedProperty _viewModelProperty;
        private SerializedProperty _sourceAdapterType;

        private ActivityBinding Component => (ActivityBinding) serializedObject.targetObject;

        private void OnEnable()
        {
            _viewModelProperty = serializedObject.FindProperty(nameof(_viewModelProperty));
            _sourceAdapterType = serializedObject.FindProperty(nameof(_sourceAdapterType));
        }

        protected override void DrawGui()
        {
            _viewModelProperty.stringValue =
                EditorUtils.DrawPopup("Model property",
                    _viewModelProperty.stringValue,
                    FindAllViewModelProperties(Component.gameObject));

            _sourceAdapterType.stringValue =
                EditorUtils.DrawPopup("Model adapter",
                    _sourceAdapterType.stringValue,
                    FindAdapterNames(typeof(bool).Name));
        }
        
        List<string> FindAllViewModelProperties(GameObject o)
        {
            var propType = typeof(bool).Name;

            Func<PropertyInfo, bool> checkProperty = info => info.PropertyType == typeof(bool);
            if (_sourceAdapterType.stringValue.NotNullOrEmpty() && _sourceAdapterType.stringValue != "None")
            {
                var adapter = FindAdapters(propType).Find(u => u.GetType().FullName == _sourceAdapterType.stringValue);
                checkProperty = info => adapter.IsAvailableSourceType(info.PropertyType);
            }
            
            bool ViewModelPropertiesFilter(PropertyInfo info) =>
                info.GetCustomAttribute<BindingAttribute>() != null && checkProperty(info);
            
            return FindAllProperties(AllAvailableViewModels(o), BindingFlags.GetProperty, ViewModelPropertiesFilter);
        }
    }
}