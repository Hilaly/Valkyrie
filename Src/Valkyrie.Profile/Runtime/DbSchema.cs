using System;

namespace Valkyrie.Profile
{
    class DbSchema
    {
        private readonly SerializationData _serializationData = new();
        private SerializationContext _serializationContext; 
        
        public Type Type { get; }
        public SerializationContext Context => _serializationContext;
        
        public DbSchema(Type type)
        {
            Type = type;
        }

        public string Serialize(object dbContext)
        {
            _serializationContext.Add(dbContext);
            return _serializationContext.Serialize();
        }

        public void Deserialize(object dbContext, string json)
        {
            if (_serializationContext == null)
                _serializationContext = new SerializationContext(_serializationData, json, dbContext);
            var typeInfo = _serializationData.GetTypeInfo(dbContext.GetType());
            //typeInfo.Deserialize(dbContext, _serializationContext.)
            
            //TODO: call deserialize method
        }
    }
}