namespace Valkyrie.Ecs
{
    public static class EcsExtensions
    {
        public static ref T GetOrCreate<T>(this EcsEntity ecsEntity) where T : struct
        {
            if (!ecsEntity.Has<T>())
                ecsEntity.Add(new T());
            return ref ecsEntity.Get<T>();
        }
    }
}