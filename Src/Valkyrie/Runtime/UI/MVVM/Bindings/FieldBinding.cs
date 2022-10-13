using System;
using System.Collections.Generic;
using System.Reflection;
using Utils;
using Valkyrie.Tools;
using Object = UnityEngine.Object;

namespace Valkyrie.MVVM.Bindings
{
    public class FieldBinding : TemplateSelector
    {
        public override bool IsValidViewModelProperty(PropertyInfo info) => info.PropertyType.GetCustomAttribute<BindingAttribute>() != null;

        public object Model { get; private set; }

        public override object ViewModelProperty
        {
            set => Model = value;
        }

        void Start()
        {
            var binding = BindViewModelProperty(_viewModelProperty, null, String.Empty, out var disposeHandler);
            this.SetBinding(nameof(ViewModelProperty), binding);

            if (_isPolling)
                UiExtension.RunPolling(disposeHandler, () =>
                {
                    if (this != null && gameObject != null && gameObject.activeInHierarchy)
                        binding.Update();
                });
        }

        static List<Type> _cachedTypes;

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