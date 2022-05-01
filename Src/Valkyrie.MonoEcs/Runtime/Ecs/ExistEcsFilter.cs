namespace Valkyrie.Ecs
{
    class ExistEcsFilter<T> : IEcsFilter where T : struct
    {
        private readonly EcsState _state;

        public ExistEcsFilter(EcsState state)
        {
            _state = state;
        }

        public bool IsMatch(EcsEntity e)
        {
            return _state.Has<T>(e);
        }

        public string GetHash() => $"EX<{typeof(T).FullName}>";
    }
}