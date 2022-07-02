using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Configs;
using Newtonsoft.Json;

namespace Valkyrie.Entities
{
    public interface IEntity : IConfigData, IDisposable
    {
    }

    public class Entity : IConfigData, IDisposable, IEntity
    {
        internal Action _finishLoadAction;

        internal void FinishLoading()
        {
            _finishLoadAction?.Invoke();
            _finishLoadAction = null;
        }
        
        #region IConfigData

        string IConfigData.GetId() => Id;

        void IConfigData.PastLoad(IDictionary<string, IConfigData> configData) { }

        #endregion

        internal readonly List<Entity> _templates = new List<Entity>();
        internal readonly List<IComponent> _components = new List<IComponent>();
        internal readonly Dictionary<string, Entity> _slots = new Dictionary<string, Entity>();

        public string Id { get; }

        public Entity(string id)
        {
            Id = id;
        }

        #region Components

        public IEnumerable<IComponent> CollectComponents(bool includeTemplates = false)
        {
            foreach (var component in _components)
                yield return component;
            if (includeTemplates)
                foreach (var template in _templates)
                foreach (var component in template.CollectComponents(true))
                    yield return component;
        }

        public IEnumerable<T> CollectComponents<T>(bool includeTemplates = false) where T : IComponent =>
            CollectComponents(includeTemplates).OfType<T>();


        public void AddComponent<T>(T c) where T : IComponent => _components.Add(c);

        public bool RemoveComponent<T>(T c) where T : IComponent
        {
            var r = _components.Remove(c);
            if (c is IDisposable d)
                d.Dispose();
            return r;
        }

        public bool HasComponent<T>(bool includeTemplates) where T : IComponent =>
            CollectComponents<T>(includeTemplates).Any();

        public T GetComponent<T>(bool includeTemplates) where T : IComponent =>
            CollectComponents<T>(includeTemplates).FirstOrDefault();

        #endregion

        #region Slots

        public void AddSlot(string name, Entity value) => _slots[name] = value;
        public bool HasSlot(string name) => _slots.ContainsKey(name);
        public Entity RemoveSlot(string name) => _slots.Remove(name, out var r) ? r : default;
        public Entity GetSlot(string name) => _slots.TryGetValue(name, out var slot) ? slot : default;

        public IEnumerable<Entity> CollectSlots()
        {
            foreach (var slot in _slots)
                if (slot.Value != null)
                    yield return slot.Value;
        }

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("{")
                .Append($" Id={Id}")
                .Append(_templates.Count > 0
                    ? $" Templates=[{string.Join(",", _templates.Select(x => x.Id))}]"
                    : string.Empty)
                .Append(_components.Count > 0
                    ? $" Components=[{string.Join(",", _components.Select(x => $"{x.GetType().Name}:{JsonConvert.SerializeObject(x, EntitiesSerializer.ComponentsSerializerSettings)}")).Replace("Component", string.Empty)}]"
                    : string.Empty)
                .Append(_slots.Count > 0
                    ? $" Slots=[{string.Join(",", _slots.Select(x => $"{x.Key}:{x.Value.Id}"))}]"
                    : string.Empty)
                .Append(" }");
            return sb.ToString();
        }

        public void Dispose()
        {
            foreach (var component in _components)
                if (component is IDisposable disposable)
                    disposable.Dispose();
        }

        public bool Is(Entity template) => template == this || _templates.Find(x => x.Is(template)) != null;
        public bool Is(string id) => Id == id || _templates.Find(x => x.Is(id)) != null;
    }
}