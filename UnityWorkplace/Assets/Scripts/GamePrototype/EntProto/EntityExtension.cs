using System.Collections.Generic;
using System.Linq;

namespace GamePrototype
{
    public static class EntityExtension
    {
        public static T GetOrCreateComponent<T>(this IEntity e) where T : new()
            => e.GetComponent<T>() ?? e.AddComponent(new T());

        public static bool HasComponent<T>(this IEntity e) => e.GetComponent<T>() != null;

        public static List<IEntity> Get<T>(this EntityContext entityContext) =>
            entityContext.Get().Where(x => HasComponent<T>(x)).ToList();
    }
}