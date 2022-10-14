using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Utils;
using Valkyrie.Tools;

namespace Valkyrie.MVVM.Bindings
{
    public class FieldBinding : AbstractBindingComponent
    {
#pragma warning disable 649
        [SerializeField] protected string _viewModelProperty;
#pragma warning restore 649

        static List<Type> _cachedTypes;
        private Bind _binding;

        public bool IsValidViewModelProperty(PropertyInfo info) =>
            info.PropertyType.GetCustomAttribute<BindingAttribute>() != null;

        private object _model;

        public object GetModel()
        {
            if (_binding == null)
            {
                _binding = BindViewModelProperty(_viewModelProperty, null, string.Empty, out var disposeHandler);
                this.SetBinding(nameof(ViewModelProperty), _binding);
            }

            _binding.Update();

            return _model;
        }

        public object ViewModelProperty
        {
            set => _model = value;
        }

        private List<Type> Types
        {
            get
            {
                if (_cachedTypes == null)
                    _cachedTypes = typeof(object).GetAllSubTypes(u =>
                        !u.IsAbstract && u.GetCustomAttribute<BindingAttribute>(false) != null);
                return _cachedTypes;
            }
        }

        public Type GetTemplateType()
        {
            if (_viewModelProperty.IsNullOrEmpty())
                return default;
            SplitTypeProperty(_viewModelProperty, out var typeName, out var propertyName);
            return Types.Find(x => x.Name == typeName)?.GetProperty(propertyName)?.PropertyType;
        }
    }
}