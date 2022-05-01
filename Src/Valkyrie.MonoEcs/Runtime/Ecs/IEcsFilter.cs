namespace Valkyrie.Ecs
{
    internal interface IEcsFilter
    {
        bool IsMatch(EcsEntity e);
        string GetHash();
    }
}