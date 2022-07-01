namespace Valkyrie.Ecs
{
    public interface IEcsState
    {
        EcsEntity GetEntity(int id);
        EcsEntity CreateEntity();
        void Destroy(int id);

        int Generate();
        
        ref T Get<T>(EcsEntity e) where T : struct;
        void Add<T>(EcsEntity e, T component) where T : struct;
        void Remove<T>(EcsEntity e) where T : struct;
        bool Has<T>(EcsEntity e) where T : struct;
        
        ref T Get<T>(int eId) where T : struct;
        void Add<T>(int eId, T component) where T : struct;
        bool Has<T>(int eId) where T : struct;
    }
}