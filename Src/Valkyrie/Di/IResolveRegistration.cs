namespace Valkyrie.Di
{
    public interface IResolveRegistration<out T>
        where T : IResolveRegistration<T>
    {
        T As<TResolveType>();
        T AsSelf();
        T AsInterfaces();
        T AsInterfacesAndSelf();
    }
}