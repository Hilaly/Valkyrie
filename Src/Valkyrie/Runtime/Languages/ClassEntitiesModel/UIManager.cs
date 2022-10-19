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
        T Model { get; }
    }

    public interface IWindowManager
    {
        Task<IUiElement<T>> ShowWindow<T>() where T : BaseWindow;
    }

    public interface IPopupManager
    {
        Task OpenPopup<T>() where T : BaseWindow;
        Task ClosePopup();
    }

    class UiElement<T> : IUiElement<T>
        where T : MonoBehaviour
    {
        private readonly T _instance;
        private IDisposable _disposable;

        public UiElement(T instance)
        {
            _instance = instance;
            _disposable = new ActionDisposable(() => _instance.gameObject.SetActive(false));
        }

        public UiElement(T instance, IDisposable disposable)
        {
            _instance = instance;
            _disposable = disposable;
        }

        public T Model
        {
            get => (T)_instance;
        }

        public void Dispose()
        {
            if (_disposable == null)
                return;
            _disposable.Dispose();
            _disposable = null;
        }
    }

    internal class UiElementsManagerBase<TWindowComponent> : MonoBehaviour
        where TWindowComponent : MonoBehaviour
    {
        [SerializeField] protected List<TWindowComponent> _windows = new();
        protected readonly CompositeDisposable _openedWindows = new();

        private void Awake()
        {
            foreach (var window in _windows)
                window.gameObject.SetActive(false);
        }

        protected TWindowComponent FindWindow(Type neededType)
        {
            var window = _windows.Find(x => x.GetType() == neededType);
            return window;
        }

        protected IUiElement<T> PrepareElement<T>(T window) where T : TWindowComponent
        {
            window.gameObject.SetActive(true);
            var r = new UiElement<T>(window);
            _openedWindows.Add(r);
            return r;
        }
    }
}