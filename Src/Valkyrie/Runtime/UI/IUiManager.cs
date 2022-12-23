using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Object = UnityEngine.Object;

namespace Valkyrie.UI
{
    public interface IWindow
    {
        int Layer { get; }

        Task Show();
        Task Hide();
    }

    [RequireComponent(typeof(Canvas))]
    public abstract class Window : MonoBehaviour, IWindow
    {
        [Inject] private UiManager _uiManager;

        [SerializeField] private int layer;

        public int Layer => layer;

        Task IWindow.Show()
        {
            gameObject.SetActive(true);
            return Task.CompletedTask;
        }

        Task IWindow.Hide()
        {
            gameObject.SetActive(false);
            return Task.CompletedTask;
        }

        [Binding]
        public async void Close() =>
            await _uiManager.OpenPrevious(Layer);
    }

    public interface IUiManager
    {
        Task<T> Open<T>() where T : IWindow;
        Task Reset();
    }

    class LayerData
    {
        public IWindow Current;
        public readonly Stack<IWindow> Windows = new();
    }

    class UiManager : IUiManager
    {
        T Get<T>() where T : IWindow
        {
            foreach (var sceneContext in Object.FindObjectsOfType<SceneContext>())
            {
                var r = sceneContext?.Container != null ? sceneContext.Container.TryResolve<T>() : default;
                if (r != null)
                    return r;
            }

            throw new Exception($"Window {typeof(T).Name} not registered in any context");
        }

        private readonly Dictionary<int, LayerData> _layers = new();

        LayerData GetLayer(int id)
        {
            if (!_layers.TryGetValue(id, out var layer))
                _layers.Add(id, layer = new LayerData());
            return layer;
        }

        public async Task<T> Open<T>() where T : IWindow
        {
            var instance = Get<T>();

            var layerInfo = GetLayer(instance.Layer);
            //Close previous
            if (layerInfo.Current != null)
            {
                if (layerInfo.Current == (IWindow)instance)
                    return instance;

                await layerInfo.Current.Hide();
                layerInfo.Windows.Push(layerInfo.Current);
                layerInfo.Current = null;
            }

            //Open new
            layerInfo.Current = instance;
            await instance.Show();

            return instance;
        }

        public async Task Reset()
        {
            foreach (var layerData in _layers.Where(x => x.Value.Current != null))
                await layerData.Value.Current.Hide();
            _layers.Clear();
        }

        public async Task OpenPrevious(int layer)
        {
            var layerInfo = GetLayer(layer);
            if (layerInfo.Current != null)
            {
                await layerInfo.Current.Hide();
                layerInfo.Current = null;
            }

            if (layerInfo.Windows.Count > 0)
            {
                layerInfo.Current = layerInfo.Windows.Pop();
                await layerInfo.Current.Show();
            }
        }
    }
}