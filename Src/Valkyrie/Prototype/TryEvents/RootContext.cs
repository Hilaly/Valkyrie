using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Di;

namespace Prototype.TryEvents
{
    public interface IRootContext
    {
        IDisposable AddHandler(IEventHandler eventHandler);

        void RaiseEvent(string eventName, params object[] args);
    }

    class RootContext : IRootContext
    {
        private readonly List<IEventHandler> _handlers = new();

        public IDisposable AddHandler(IEventHandler eventHandler)
        {
            _handlers.Add(eventHandler);
            return new ActionDisposable(() => _handlers.Remove(eventHandler));
        }

        public void RaiseEvent(string eventName, params object[] args)
        {
            for (var index = 0; index < _handlers.Count; index++)
                if (_handlers[index].Test(eventName, args))
                    return;

            Debug.LogWarning($"[RCtx]: '{eventName}' not handled");
        }
    }
}