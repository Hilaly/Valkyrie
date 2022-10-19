using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.MVVM.Bindings;

namespace Valkyrie
{
    [Binding]
    public abstract class BaseWindow : MonoBehaviour
    {
        [Inject] private IEventSystem _eventSystem;

        protected Task Raise<T>(T instance) where T : BaseEvent
        {
            Debug.Log($"[GEN]: Raise {typeof(T).Name} event from {GetType().Name}");
            return _eventSystem.Raise(instance);
        }
    }
    
    public interface IUiElement<T> : IDisposable
    {
        T Model { get; set; }
    }

    public interface IWindowManager
    {
        Task<IUiElement<T>> ShowWindow<T>() where T : BaseWindow;
    }

    public interface IPopupManager
    {
        Task OpenPopup<T>();
        Task ClosePopup();
    }

    class UiElement<T> : IUiElement<T>
    {
        private readonly TypeBinding _instance;
        private IDisposable _disposable;

        public UiElement(TypeBinding instance)
        {
            _instance = instance;
            _disposable = new ActionDisposable(() => _instance.gameObject.SetActive(false));
        }

        public UiElement(TypeBinding instance, IDisposable disposable)
        {
            _instance = instance;
            _disposable = disposable;
        }

        public T Model
        {
            get => (T)_instance.Model;
            set => _instance.Model = value;
        }

        public void Dispose()
        {
            if (_disposable == null)
                return;
            _disposable.Dispose();
            _disposable = null;
        }
    }

    internal class UiElementsManagerBase : MonoBehaviour
    {
        [SerializeField] protected List<TypeBinding> _windows = new();
        protected readonly CompositeDisposable _openedWindows = new();

        private void Awake()
        {
            foreach (var window in _windows)
                window.gameObject.SetActive(false);
        }

        protected TypeBinding FindWindow(Type neededType)
        {
            var window = _windows.Find(x => x.GetTemplateType() == neededType);
            return window;
        }

        protected IUiElement<T> PrepareElement<T>(TypeBinding window)
        {
            window.gameObject.SetActive(true);
            var r = new UiElement<T>(window);
            _openedWindows.Add(r);
            return r;
        }
    }
}