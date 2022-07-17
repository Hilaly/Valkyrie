using UnityEngine;

namespace Valkyrie.MVVM.Bindings
{
    public class ActivityBinding : AbstractBindingComponent
    {
#pragma warning disable 649
        [SerializeField] string _viewModelProperty;

        [SerializeField] private bool _isPolling = true;

        [SerializeField] private string _sourceAdapterType = "None";
#pragma warning restore 649

        void Start()
        {
            var binding = BindViewModelProperty(_viewModelProperty, null, _sourceAdapterType, out var disposeHandler);

            this.SetBinding(nameof(GameObjectActive), binding);
            if (_isPolling)
                DataExtensions.RunPolling(disposeHandler, () =>
                {
                    if (this != null && gameObject != null)
                        binding.Update();
                });
        }

        bool GameObjectActive
        {
            set => gameObject.SetActive(value);
        }
    }
}