using System;
using Newtonsoft.Json.Linq;

namespace Valkyrie.Profile
{
    internal class TypeSerializationInfo
    {
        private readonly Func<TypeSerializationInfo, object, object> _getIdCall;
        private Func<TypeSerializationInfo, object, JObject> _serializeCall;
        private Action<TypeSerializationInfo, JObject, object> _deserializeCall;
        private Action<SerializationContext, object> _prepareReferencesCall;
        private Action<SerializationContext, object, JObject> _setReferencesCall;
        private readonly SerializationData _root;

        public Type Type { get; }
        public Type IdType { get; }
        public string Table { get; }

        public ulong Id { get; set; } = 1;

        public TypeSerializationInfo(SerializationData serRoot, Type type)
        {
            Type = type;
            _root = serRoot;

            Table = SerializationUtils.GetTableName(type);
            _getIdCall = SerializationUtils.GetIdMethod(type, out var idType);
            IdType = idType;
        }

        public object GetId(object o) => _getIdCall(this, o);
        public JObject Serialize(object o) =>
            (_serializeCall ??= SerializationUtils.SerializeMethod(_root, Type)).Invoke(this, o);
        public void Deserialize(object o, JObject json) =>
            (_deserializeCall ??= SerializationUtils.DeserializeMethod(Type)).Invoke(this, json, o);
        public object Deserialize(JObject json)
        {
            var o = Activator.CreateInstance(Type);
            Deserialize(o, json);
            return o;
        }

        public void PrepareDb(SerializationContext serializationContext, object o) =>
            (_prepareReferencesCall ??= SerializationUtils.PrepareReferencesMethod(_root, Type)).Invoke(
                serializationContext, o);

        public void SetReferences(SerializationContext serializationContext, object o, JObject json) =>
            (_setReferencesCall ??= SerializationUtils.SetReferencesMethod(_root, Type)).Invoke(serializationContext, o,
                json);
    }
}