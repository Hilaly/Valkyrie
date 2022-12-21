namespace Valkyrie.Meta.DataSaver
{
    public interface IModelsProvider
    {
        T Add<T>(T value) where T : BaseModel;
        T Get<T>() where T : BaseModel;
    }
}