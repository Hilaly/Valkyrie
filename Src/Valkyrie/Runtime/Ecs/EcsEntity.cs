namespace Valkyrie.Ecs
{
    public struct EcsEntity
    {
        public int Id;
        public IEcsState State;

        public ref T Get<T>() where T : struct => ref State.Get<T>(this);
        public bool Has<T>() where T : struct => State.Has<T>(this);
        public void Add<T>(T component) where T : struct => State.Add(this, component);
        public void Remove<T>() where T : struct => State.Remove<T>(this);

        public void Destroy() => State.Destroy(Id);
    }
}