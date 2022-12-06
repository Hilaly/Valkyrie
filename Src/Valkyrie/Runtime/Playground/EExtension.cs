using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Valkyrie.Playground
{
    public static class EExtension
    {
        public static IReadOnlyList<T> GetAll<T>(this IComponent component) where T : IComponent =>
            component.Entity.GetAll<T>();

        public static T Get<T>(this IComponent component) where T : IComponent =>
            component.Entity.Get<T>();
    }
}