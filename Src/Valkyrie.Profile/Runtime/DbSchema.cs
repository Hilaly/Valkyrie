namespace Valkyrie.Profile
{
    internal class DbSchema
    {
        private readonly SerializationData _serializationData = new();

        public SerializationContext Context { get; private set; }

        public string Serialize(object dbContext)
        {
            Context ??= new SerializationContext(_serializationData, dbContext);
            Context.Add(dbContext);
            return Context.Serialize();
        }

        public void Deserialize(object dbContext, string json)
        {
            Context = new SerializationContext(_serializationData, json, dbContext);
        }
    }
}