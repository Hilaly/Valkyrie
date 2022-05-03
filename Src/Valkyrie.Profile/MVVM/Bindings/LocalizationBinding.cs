using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public class LocalizationBinding : AbstractBindingComponent
    {
#pragma warning disable 649
        [SerializeField] private string _localizeKey;
        [SerializeField] List<string> _viewModelProperties = new List<string>();
        
        //[Inject] private ILocalization _localization;
#pragma warning restore 649

        /*
        private Action _updateSource;
        private string _localizedText = string .Empty;
        private Action _updateTarget;

        [Inject] void Init()
        {
            List<object> values = new List<object>();
            for (var i = 0; i < _viewModelProperties.Count; ++i)
                values.Add(null);
            List<Func<object>> getters = new List<Func<object>>();
            for (var i = 0; i < _viewModelProperties.Count; ++i)
            {
                SplitTypeProperty(_viewModelProperties[i], out var typeName, out var propertyName);
                var viewModel = GetModel(gameObject, typeName, out var disposeHandler);
                var property = viewModel.GetType().GetProperty(propertyName);
                getters.Add(() => property.GetValue(viewModel, null));
            }
            
            _updateSource = () =>
            {
                var changed = _localizedText.IsNullOrEmpty();
                for (var i = 0; i < _viewModelProperties.Count; ++i)
                {
                    var newValue = getters[i].Invoke();
                    if (values[i] == newValue)
                        continue;
                    values[i] = newValue;
                    changed = true;
                }

                if (changed)
                    _localizedText = _localization.GetFormattedString(_localizeKey, (IEnumerable<object>)values);
            };
            
            var text = GetComponent<Text>();
            if (text != null)
                _updateTarget = () => text.text = LocalizedText;
            
            else
            {
                var textMeshPro = GetComponent<TextMeshPro>();
                if (textMeshPro != null)
                    _updateTarget = () => textMeshPro.text = LocalizedText;
                else
                {
                    var textMeshProUi = GetComponent<TextMeshProUGUI>();
                    _updateTarget = () => textMeshProUi.text = LocalizedText;
                }
            }
            
            LateUpdate();
        }

        private void LateUpdate()
        {
            //TODO: must not generate garbage
            _updateSource();
            _updateTarget();
        }

        public string LocalizedText => _localizedText;
        */
    }
}