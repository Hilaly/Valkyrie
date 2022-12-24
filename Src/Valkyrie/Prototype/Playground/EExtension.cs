using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Valkyrie.Playground
{
    public static class EExtension
    {
        public static IReadOnlyList<T> GetAll<T>(this IComponent component) where T : IComponent =>
            component.Entity.GetAll<T>();

        public static T Get<T>(this IComponent component) where T : IComponent =>
            component.Entity.Get<T>();

        public static T GetOrCreate<T>(this IEntity e) where T : MonoComponent
        {
            var r = e.Get<T>();
            if (r == null)
                r = e.Add<T>();
            return r;
        }

        public static T SendEvent<T>(this GameState gameState, T eventInstance) where T : class, IEventComponent =>
            new EventEntity<T>(eventInstance, gameState).Get<T>();

        public static T SendEvent<T>(this GameState gameState) where T : class, IEventComponent, new() =>
            SendEvent(gameState, new T());

        internal static ISystem CreateEventClearSystem(this IWorld world, Type evType)
        {
            var systemType = typeof(EventClearSystem<>).MakeGenericType(evType);
            var systemInstance = Activator.CreateInstance(systemType, world);
            var result = (ISystem)Activator.CreateInstance(
                typeof(ProfileSystem<>).MakeGenericType(systemType),
                systemInstance, world);
            return result;
        }

        public static object Add(this IEntity entity, Type component)
        {
            return (entity as EntityBehaviour)?.gameObject.AddComponent(component);
        }
    }
}