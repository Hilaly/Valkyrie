using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public class TemplateBinding : TemplateSelector
    {
        private object _lastValue;
        private GameObject _createdView;

        public override bool IsValidViewModelProperty(PropertyInfo info)
        {
            return !typeof(IEnumerable).IsAssignableFrom(info.PropertyType);
        }

        public override object ViewModelProperty
        {
            set
            {
                if (value == _lastValue)
                    return;

                if (_lastValue is IViewOwner lastOwner)
                    lastOwner.View = null;

                _lastValue = value;

                //Destroy old
                if (_createdView != null)
                    _createdView = RemoveTemplate(_createdView);

                if (_lastValue != null)
                    _createdView = SpawnTemplate(_lastValue);

                if (_lastValue is IViewOwner owner)
                    owner.View = _createdView;
            }
        }
    }
}