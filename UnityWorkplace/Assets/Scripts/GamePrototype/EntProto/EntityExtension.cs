using System.Collections.Generic;
using System.Linq;

namespace NaiveEntity.GamePrototype.EntProto
{
    public static class EntityExtension
    {
        public static T GetOrCreateComponent<T>(this IEntity e) where T : new()
            => e.GetComponent<T>() ?? e.AddComponent(new T());

        public static bool HasComponent<T>(this IEntity e) => e.GetComponent<T>() != null;

        public static List<IEntity> Get<T>(this EntityContext entityContext) =>
            entityContext.Get().Where(HasComponent<T>).ToList();

        public static bool WillRespondTo<T>(this IEntity e) where T : class =>
            ((Entity)e).Components.Find(x => x is IEventConsumer<T>) != null;
        
        public static void PropagateEvent<T>(this IEntity e, T ev) where T : class
        {
            foreach (var consumer in ((Entity)e).Components.OfType<IEventConsumer<T>>()) 
                consumer.PropagateEvent(e, ev);
        }
    }
}