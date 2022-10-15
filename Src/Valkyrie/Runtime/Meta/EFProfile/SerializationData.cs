using System;
using System.Collections.Generic;

namespace Valkyrie.Profile
{
    internal class SerializationData
    {
        private readonly Dictionary<Type, TypeSerializationInfo> _serializationInfos = new();

        public TypeSerializationInfo GetTypeInfo(Type type)
        {
            lock (_serializationInfos)
            {
                if (!_serializationInfos.TryGetValue(type, out var result))
                    _serializationInfos.Add(type, result = new TypeSerializationInfo(this, type));

                return result;
            }
        }

        public TypeSerializationInfo GetTypeInfo(string tableName)
        {
            lock (_serializationInfos)
            {
                foreach (var (_, value) in _serializationInfos)
                {
                    if (value.Table == tableName)
                        return value;
                }

                return default;
            }
        }
    }
}