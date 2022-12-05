using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Valkyrie.Playground
{
    public static class EExtension
    {
        public static IReadOnlyList<T> Get<T>(this IComponent component) where T : IComponent =>
            component.Entity.Get<T>();

        public static T Single<T>(this IComponent component) where T : IComponent =>
            component.Get<T>().FirstOrDefault();

        public static T Single<T>(this IEntity e) where T : IComponent =>
            e.Get<T>().FirstOrDefault();
    }
}