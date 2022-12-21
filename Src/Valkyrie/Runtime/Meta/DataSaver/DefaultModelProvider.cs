namespace Valkyrie.Meta.DataSaver
{
    class DefaultModelProvider<T> where T : BaseModel, new()
    {
        private readonly IModelsProvider _modelsProvider;

        public T Model => _modelsProvider.Get<T>() ?? _modelsProvider.Add(new T());

        public DefaultModelProvider(IModelsProvider modelsProvider)
        {
            _modelsProvider = modelsProvider;
        }
    }
}