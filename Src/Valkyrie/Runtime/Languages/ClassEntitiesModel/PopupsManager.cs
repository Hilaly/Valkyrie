using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Valkyrie
{
    class PopupsManager : UiElementsManagerBase<BaseWindow>, IPopupManager
    {
        private readonly List<Func<Task>> _queue = new();

        public Task OpenPopup<T>() where T : BaseWindow
        {
            if (!_openedWindows.Any())
            {
                var neededType = typeof(T);
                var window = (T)FindWindow(neededType);
                if (window == null)
                {
                    Debug.LogWarning($"Popup of type {neededType.FullName} not registered in window manager");
                    return ClosePopup();
                }

                PrepareElement<T>(window);
            }
            else
                _queue.Add(OpenPopup<T>);

            return Task.CompletedTask;
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