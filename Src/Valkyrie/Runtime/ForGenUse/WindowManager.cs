using System;
using System.Threading.Tasks;

namespace Valkyrie
{
    class WindowManager : UiElementsManagerBase<BaseWindow>, IWindowManager
    {
        public Task<IUiElement<BaseWindow>> ShowWindow(Type neededType)
        {
            var window = FindWindow(neededType);
            if (window == null)
                throw new ArgumentException($"Window of type {neededType.FullName} not registered in window manager");
            _openedWindows.Dispose();
            return Task.FromResult(PrepareElement(window));
        }

        public Task<IUiElement<T>> ShowWindow<T>() where T : BaseWindow
        {
            var neededType = typeof(T);
            var window = (T)FindWindow(neededType);
            if (window == null)
                throw new ArgumentException($"Window of type {neededType.FullName} not registered in window manager");
            _openedWindows.Dispose();
            return Task.FromResult(PrepareElement(window));
        }
    }
}