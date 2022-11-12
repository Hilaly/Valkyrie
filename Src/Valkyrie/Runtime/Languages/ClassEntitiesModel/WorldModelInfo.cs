using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Valkyrie
{
    public partial class WorldModelInfo : Feature
    {
        public string Namespace;

        public List<EventEntity> Events = new();
        public List<WindowType> Windows = new();
        public ProfileModel Profile = new();

        public WorldModelInfo()
        {
            Namespace = EditorSettings.projectGenerationRootNamespace;
            name = PlayerSettings.applicationIdentifier;
            displayName = Application.productName;
            description = "Main context for project generation";
        }

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

        public WindowType GetWindow(string name)
        {
            var r = Windows.Find(x => x.Name == name);
            if (r == null)
                Windows.Add(r = new WindowType() { Name = name });
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

        public WorldModelInfo Import(IFeature feature)
        {
            feature.Import(this);
            return this;
        }
    }
}