using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Configs;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.Ecs;

namespace Valkyrie
{
    [Binding]
    public abstract class BaseView
    {
        [Inject] private IEventSystem _events;

        [field: Inject] public IConfigService Config { get; }
        [field: Inject] public ICommandsInterpreter Interpreter { get; }

        protected Task Raise<T>(T instance) where T : BaseEvent
        {
            Debug.Log($"[GEN]: Raise {typeof(T).Name} event from {GetType().Name}");
            return _events.Raise(instance);
        }
    }
    
    [Binding]
    public abstract class BaseWindow : MonoBehaviour
    {
        [Inject] private IEventSystem _events;

        [field: Inject] public IConfigService Config { get; }
        [field: Inject] public ICommandsInterpreter Interpreter { get; }

        protected Task Raise<T>(T instance) where T : BaseEvent
        {
            Debug.Log($"[GEN]: Raise {typeof(T).Name} event from {GetType().Name}");
            return _events.Raise(instance);
        }
    }
    
    public interface IUiElement<out T> : IDisposable
    {
        T Model { get; }
    }

    public interface IWindowManager
    {
        Task ShowWindow(Type type);
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
        private readonly HashSet<TWindowComponent> _filter = new();
        protected bool IsAwakened { get; private set; }

        protected virtual void Awake()
        {
            foreach (var window in _windows.Where(x => !_filter.Contains(x)))
                window.gameObject.SetActive(false);
            _filter.Clear();
            IsAwakened = true;
        }

        protected TWindowComponent FindWindow(Type neededType)
        {
            var window = _windows.Find(x => x.GetType() == neededType);
            if (!IsAwakened)
                _filter.Add(window);
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

    public class UiCommands : IDisposable
    {
        private readonly CompositeDisposable _compositeDisposable = new();
        private readonly IWindowManager _windowManager;
        private readonly IPopupManager _popupManager;
        private readonly Dictionary<string,Type> _validWindows;

        public UiCommands(IWindowManager windowManager, IPopupManager popupManager, ICommandsInterpreter interpreter)
        {
            _windowManager = windowManager;
            _popupManager = popupManager;

            _validWindows = typeof(BaseWindow).GetAllSubTypes(x => x.IsClass && !x.IsAbstract).ToDictionary(x => x.Name, x => x);
            
            _compositeDisposable.Add(interpreter.Register<string>("ShowWindow", ShowWindow));
        }

        async Task ShowWindow(string windowName)
        {
            if (_validWindows.TryGetValue(windowName, out var windowType))
            {
                Debug.Log($"[GEN]: opening {windowName}");
                await _windowManager.ShowWindow(windowType);
            }
            else
            {
                var msg = $"[GEN]: {windowName} is not registered window type";
                //Debug.LogError(msg);
                throw new Exception(msg);
            }
        }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }
    }
}