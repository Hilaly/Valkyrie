using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Meta;
using UnityEditor;
using UnityEngine;
using Utils;
using Valkyrie.MVVM.Bindings;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Editor
{
    [CustomEditor(typeof(AbstractViewBinding), true)]
    public class AbstractPropertyBindingEditor : AbstractBindingEditor
    {
        private SerializedProperty _isTwoSided;
        private SerializedProperty _viewProperty;
        private SerializedProperty _viewModelProperty;
        private SerializedProperty _sourceAdapterType;

        private AbstractViewBinding Component => (AbstractViewBinding) serializedObject.targetObject;

        private void OnEnable()
        {
            _isTwoSided = serializedObject.FindProperty(nameof(_isTwoSided));
            _viewProperty = serializedObject.FindProperty(nameof(_viewProperty));
            _viewModelProperty = serializedObject.FindProperty(nameof(_viewModelProperty));
            _sourceAdapterType = serializedObject.FindProperty(nameof(_sourceAdapterType));
        }

        protected override void DrawGui()
        {
            EditorGUILayout.PropertyField(_isTwoSided, new GUIContent("Is Two Way Binding"));
            
            //View property
            _viewProperty.stringValue =
                EditorUtils.DrawPopup("View property",
                    _viewProperty.stringValue,
                    FindAllViewProperties(Component.gameObject));

            _sourceAdapterType.stringValue =
                EditorUtils.DrawPopup("Model adapter",
                    _sourceAdapterType.stringValue,
                    FindAdapters());
            
            //ViewModel property
            _viewModelProperty.stringValue =
                EditorUtils.DrawPopup("Model property",
                    _viewModelProperty.stringValue,
                    FindAllViewModelProperties(Component.gameObject));
        }

        private List<string> FindAdapters()
        {
            if (_viewProperty.stringValue.IsNullOrEmpty())
                return new List<string>();

            var propType = _viewProperty.stringValue.Substring(_viewProperty.stringValue.LastIndexOf(':') + 1);

            return FindAdapterNames(propType);
        }

        List<string> FindAllViewModelProperties(GameObject o)
        {
            if (_viewProperty.stringValue.IsNullOrEmpty())
                return new List<string>();
            
            var propType = _viewProperty.stringValue.Substring(_viewProperty.stringValue.LastIndexOf(':') + 1);

            Func<PropertyInfo, bool> checkProperty = info => info.PropertyType.Name == propType;
            if (_sourceAdapterType.stringValue.NotNullOrEmpty() && _sourceAdapterType.stringValue != "None")
            {
                var adapter = FindAdapters(propType).Find(u => u.GetType().FullName == _sourceAdapterType.stringValue);
                checkProperty = info => adapter.IsAvailableSourceType(info.PropertyType);
            }
            
            var flags = _isTwoSided.boolValue
                ? BindingFlags.GetProperty | BindingFlags.SetProperty
                : BindingFlags.GetProperty;

            bool ViewModelPropertiesFilter(PropertyInfo info) =>
                info.GetCustomAttribute<BindingAttribute>() != null && checkProperty(info);
            
            return FindAllProperties(AllAvailableViewModels(o), flags, ViewModelPropertiesFilter);
        }

        List<string> FindAllViewProperties(GameObject o)
        {
            var flags = _isTwoSided.boolValue
                ? BindingFlags.SetProperty | BindingFlags.GetProperty
                : BindingFlags.SetProperty;
            
            bool ViewPropertiesFilter(PropertyInfo info) =>
                info.CanWrite && ViewModelPropertyFilter(info) &&
                (!_isTwoSided.boolValue || info.CanRead);

            return FindAllProperties(AllAvailableViews(Component).Select(u => u.GetType()), flags,
                ViewPropertiesFilter);
        }

        bool ViewModelPropertyFilter(PropertyInfo info)
        {
            return ViewPropertiesTypes.Contains(info.PropertyType)
                   || typeof(UnityEngine.Object).IsAssignableFrom(info.PropertyType)
                   || info.PropertyType.GetCustomAttribute<IsValidBindingTypeAttribute>() != null;
        }
    }
}