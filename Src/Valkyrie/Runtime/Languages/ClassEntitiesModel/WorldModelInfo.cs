using System.Collections.Generic;
using System.Linq;

namespace Valkyrie
{
    public abstract class MainData
    {
        public string name;
        public string displayName;
        public string description;

        public Dictionary<string, string> dependencies = new();
    }

    public class Feature : MainData
    {
        private List<BaseType> Types = new();

        public T Get<T>(string name) where T : BaseType => (T)Types.Find(x => x is T && x.Name == name);

        private T GetOrCreate<T>(string name) where T : BaseType, new()
        {
            var r = Get<T>(name);
            if (r == null)
                Types.Add(r = new T { Name = name });
            return r;
        }

        public IReadOnlyList<T> Get<T>() where T : BaseType => Types.OfType<T>().ToList();

        public EntityType CreateEntity(string name) => GetOrCreate<EntityType>(name);
        public ConfigType CreateConfig(string name) => GetOrCreate<ConfigType>(name);
        public ItemType CreateItem(string name) => GetOrCreate<ItemType>(name);
    }

    public class WorldModelInfo : Feature
    {
        public string Namespace = nameof(WorldModelInfo);

        public List<EventEntity> Events = new();
        public List<WindowModelInfo> Windows = new();
        public ProfileModel Profile = new();

        #region Request

        internal bool IsEntityInterface(BaseType baseType)
        {
            if (baseType is EntityType entityType)
                return Get<EntityType>().Any(x => x.BaseTypes.Contains(entityType));
            return false;
        }

        internal bool IsEntityClass(BaseType baseType)
        {
            if (baseType is EntityType entityType)
                return Get<EntityType>().All(x => !x.BaseTypes.Contains(entityType));
            return false;
        }

        internal List<(string, BaseType)> GetAllTimers()
        {
            var allTimers = Get<EntityType>().SelectMany(entityType =>
            {
                return entityType.Timers.Select(x =>
                    (x, (BaseType)entityType) /* new TimerData
                    {
                        Timer = x,
                        Type = entityType
                    }*/);
            }).ToList();
            return allTimers;
        }

        #endregion

        #region Utils

        public override string ToString() => this.ToString(true);

        public void Parse(string source) => WorldModelCompiler.Parse(this, source);

        public void WriteToDirectory(string dirPath, string fileName = "Gen.cs") =>
            TypesToCSharpSerializer.WriteToDirectory(this, dirPath, fileName);

        #endregion

        #region Old to refactor

        public EventEntity CreateEvent(string eventName, params string[] args)
        {
            var r = new EventEntity { Name = eventName };
            r.Args.AddRange(args);
            Events.Add(r);
            return r;
        }

        public EventHandlerModel CreateEventHandler(EventEntity evToHandle)
        {
            var r = new EventHandlerModel(evToHandle);
            Profile.Handlers.Add(r);
            return r;
        }

        public WindowModelInfo GetWindow(string name)
        {
            var r = Windows.Find(x => x.Name == name);
            if (r == null)
                Windows.Add(r = new WindowModelInfo() { Name = name });
            return r;
        }

        public ItemType GetItem(string name)
        {
            var r = Profile.Items.Find(x => x.Name == name);
            if (r == default)
            {
                Profile.Items.Add(r = new ItemType() { Name = name });
                CreateFilter($"AllOf{r.Name}", r);
            }

            return r;
        }

        public ItemFilterModel CreateFilter(string name, ItemType itemEntity)
        {
            var r = new ItemFilterModel() { Entity = itemEntity, Name = name };
            Profile.Filters.Add(r);
            return r;
        }

        #endregion
    }
}