namespace Valkyrie.Di
{
    interface IContainerResolver
    {
        string Name { get; }
        object Resolve(ResolvingArguments args);
    }
}