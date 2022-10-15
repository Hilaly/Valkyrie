using UnityEngine;
using Utils;

namespace Valkyrie.MVVM.Bindings
{
    public abstract class AbstractViewBinding : AbstractBindingComponent
    {
#pragma warning disable 649
        [SerializeField] private bool _isTwoSided;

        [SerializeField] private string _viewProperty;
        [SerializeField] string _viewModelProperty;

        [SerializeField] string _changeEventName;
        [SerializeField] private bool _isPolling = true;

        [SerializeField] private string _sourceAdapterType = "None";
#pragma warning restore 649

        void Start()
        {
            var binding = this.BindViewModelProperty(_viewModelProperty, _changeEventName, _sourceAdapterType,
                out var disposeHandler);

            Bind(binding);

            if (_isTwoSided)
                binding.SetTwoSided().AttachTo(disposeHandler);
            if (_isPolling)
                UiExtension.RunPolling(disposeHandler, () =>
                {
                    if (this != null && gameObject != null && gameObject.activeInHierarchy)
                        binding.Update();
                });
        }

        protected virtual void Bind(Bind binding)
        {
            SplitTypeProperty(_viewProperty, out var typeName, out var propertyName);
            var component = gameObject.GetComponent(typeName);
            component.SetBinding(propertyName, binding);
        }
    }
}