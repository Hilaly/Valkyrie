using System;
using System.Threading.Tasks;

namespace Valkyrie
{
    class WindowManager : UiElementsManagerBase, IWindowManager
    {
        public Task<IUiElement<T>> ShowWindow<T>()
        {
            var neededType = typeof(T);
            var window = FindWindow(neededType);
            if (window == null)
                throw new ArgumentException($"Window of type {neededType.FullName} not registered in window manager");
            _openedWindows.Dispose();
            return Task.FromResult(PrepareElement<T>(window));
        }
    }
}