namespace Valkyrie.Ecs
{
    class NotExistEcsFilter<T> : IEcsFilter where T : struct
    {
        private readonly EcsState _state;

        public NotExistEcsFilter(EcsState state)
        {
            _state = state;
        }

        public bool IsMatch(EcsEntity e)
        {
            return !_state.Has<T>(e);
        }
        
        public string GetHash() => $"NOT<{typeof(T).FullName}>";
    }
}