using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static T _instance;

        public static T Instance => _instance ??= new T();

        protected Singleton()
        {
            if (_instance != null && _instance != this)
                throw new Exception($"Instance of {nameof(T)} already exists");
            _instance = this as T;
        }
    }

    public interface IEventSystem
    {
        IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : BaseEvent;
        Task Raise<TEvent>(TEvent ev) where TEvent : BaseEvent;
    }

    public static class EventsExtensions
    {
        public static IDisposable Subscribe<TEvent>(this IEventSystem eventSystem, Action<TEvent> handler)
            where TEvent : BaseEvent =>
            eventSystem.Subscribe<TEvent>(ev =>
            {
                handler(ev);
                return Task.CompletedTask;
            });
    }

    public class EventSystem : Singleton<EventSystem>, IEventSystem
    {
        private readonly Dictionary<Type, List<object>> _handlers = new();

        List<object> Get(Type type)
        {
            if (!_handlers.TryGetValue(type, out var list))
                _handlers.Add(type, list = new List<object>());
            return list;
        }

        public IDisposable Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : BaseEvent
        {
            var list = Get(typeof(TEvent));
            list.Add(handler);
            return new ActionDisposable(() => list.Remove(handler));
        }

        public async Task Raise<TEvent>(TEvent ev) where TEvent : BaseEvent
        {
            var temp = Get(typeof(TEvent));
            if (temp.Count == 0)
            {
                Debug.LogWarning($"[EVENT]: Unhandled event {ev}");
            }
            var list = temp.ConvertAll(x => (Func<TEvent, Task>)x);
            for (var i = 0; i < list.Count; ++i) 
                await list[i].Invoke(ev);
        }
    }

    public abstract class BaseEvent
    {
    }

    public abstract class BaseEvent<T> : BaseEvent
    {
        public T Arg0;
    }

    public abstract class BaseEvent<T1, T2> : BaseEvent
    {
        public T1 Arg0;
        public T2 Arg1;
    }
}