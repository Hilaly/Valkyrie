using System;
using Newtonsoft.Json.Linq;

namespace Valkyrie.Profile
{
    class TypeSerializationInfo
    {
        private readonly Func<TypeSerializationInfo, object, object> _getIdCall;
        private Func<TypeSerializationInfo, object, JObject> _serializeCall;
        private Action<TypeSerializationInfo, JObject, object> _deserializeCall;
        
        public Type Type { get; }
        public Type IdType { get; }
        public string Table { get; }

        public SerializationData Root { get; }
        public ulong Id { get; set; } = 1;

        public TypeSerializationInfo(SerializationData serRoot, Type type)
        {
            Type = type;
            Root = serRoot;

            Table = SerializationUtils.GetTableName(type);
            _getIdCall = SerializationUtils.GetIdMethod(type, out var idType);
            IdType = idType;
        }

        public object GetId(object o) => _getIdCall(this, o);

        public JObject Serialize(object o) =>
            (_serializeCall ??= SerializationUtils.SerializeMethod(Root, Type)).Invoke(this, o);

        public void Deserialize(object o, JObject json) =>
            (_deserializeCall ??= SerializationUtils.DeserializeMethod(Root, Type)).Invoke(this, json, o);

        public object Deserialize(JObject json)
        {
            var o = Activator.CreateInstance(Type);
            Deserialize(o, json);
            return o;
        }
            

        public void SetReferences(SerializationContext serializationContext, object o, JObject json)
        {
            return;
        }
    }
}