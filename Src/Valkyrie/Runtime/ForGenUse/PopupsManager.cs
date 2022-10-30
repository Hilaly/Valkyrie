using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Valkyrie
{
    class PopupsManager : UiElementsManagerBase<BaseWindow>, IPopupManager
    {
        private readonly List<Func<Task>> _queue = new();

        protected override void Awake()
        {
            base.Awake();
            ClosePopup();
        }

        public async Task<IUiElement<BaseWindow>> OpenPopup(Type neededType)
        {
            if (IsAwakened && !_openedWindows.Any())
            {
                var window = FindWindow(neededType);
                if (window == null)
                {
                    Debug.LogWarning($"Popup of type {neededType.FullName} not registered in window manager");
                    await ClosePopup();
                }

                return PrepareElement(window);
            }

            var tcs = new TaskCompletionSource<IUiElement<BaseWindow>>();

            _queue.Add(async () =>
            {
                var result = await OpenPopup(neededType);
                tcs.SetResult(result);
            });

            return await tcs.Task;
        }

        public async Task<IUiElement<T>> OpenPopup<T>() where T : BaseWindow
        {
            if (IsAwakened && !_openedWindows.Any())
            {
                var neededType = typeof(T);
                var window = (T)FindWindow(neededType);
                if (window == null)
                {
                    Debug.LogWarning($"Popup of type {neededType.FullName} not registered in window manager");
                    await ClosePopup();
                }

                return PrepareElement(window);
            }

            var tcs = new TaskCompletionSource<IUiElement<T>>();

            _queue.Add(async () =>
            {
                var result = await OpenPopup<T>();
                tcs.SetResult(result);
            });

            return await tcs.Task;
        }

        public Task ClosePopup()
        {
            _openedWindows.Dispose();
            if (_queue.Count > 0)
            {
                var f = _queue[0];
                _queue.RemoveAt(0);
                return f.Invoke();
            }

            return Task.CompletedTask;
        }
    }
}