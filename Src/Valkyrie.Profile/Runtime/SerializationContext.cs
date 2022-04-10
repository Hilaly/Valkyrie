using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Valkyrie.Profile
{
    internal class SerializationContext
    {
        private readonly SerializationData _serializationData;
        private readonly Dictionary<string, TableInfo> _values;
        private readonly Dictionary<string, Dictionary<string, object>> _parsedValues = new();
        private readonly HashSet<Type> _unsupportedTypes = new();

        public SerializationContext(SerializationData serializationData, string json, object dbContext)
        {
            _serializationData = serializationData;
            _values = JsonConvert.DeserializeObject<Dictionary<string, TableInfo>>(json);
            _unsupportedTypes.Add(dbContext.GetType());

            TryDeserializeObject(dbContext);
        }

        public SerializationContext(SerializationData serializationData, object dbContext)
        {
            _serializationData = serializationData;
            _values = new Dictionary<string, TableInfo>();
            _unsupportedTypes.Add(dbContext.GetType());

            Add(dbContext);
        }

        private void TryDeserializeObject(object value)
        {
            var tableType = value.GetType();
            var typeInfo = _serializationData.GetTypeInfo(tableType);
            var tableName = typeInfo.Table;
            if (_values.TryGetValue(tableName, out var unparsedList) && unparsedList.Values.Count > 0)
            {
                var pair = unparsedList.Values[^1];
                var jo = JObject.Parse(pair.Value);
                typeInfo.Deserialize(value, jo);
                typeInfo.SetReferences(this, value, jo);
                
                Add(value);
            }
        }

        public Dictionary<string, object> Get(Type tableType)
        {
            var typeInfo = _serializationData.GetTypeInfo(tableType);
            if (!_parsedValues.TryGetValue(typeInfo.Table, out var parsedList))
            {
                _parsedValues.Add(typeInfo.Table, parsedList = new Dictionary<string, object>());
                if (_values.TryGetValue(typeInfo.Table, out var unparsedList))
                    Deserialize(unparsedList, typeInfo, parsedList);
            }

            return parsedList;
        }

        private void Deserialize(TableInfo unparsedList, TypeSerializationInfo typeInfo, IDictionary<string, object> parsedList)
        {
            if(_unsupportedTypes.Contains(typeInfo.Type))
                return;
            
            typeInfo.Id = unparsedList.Id;
            var calls = new List<Action>();
            foreach (var pair in unparsedList.Values)
            {
                var strId = pair.Key;
                var jo = JObject.Parse(pair.Value);

                if (parsedList.TryGetValue(strId, out var value))
                    typeInfo.Deserialize(value, jo);
                else
                {
                    value = typeInfo.Deserialize(jo);
                    var key = typeInfo.GetId(value);
                    parsedList.Add(key.ToString(), value);
                }

                calls.Add(() => typeInfo.SetReferences(this, value, jo));
            }

            foreach (var call in calls) call();
        }

        public string Serialize()
        {
            //Fill with all exist objects
            foreach (var parsedValue in _parsedValues.ToList())
            {
                var table = _serializationData.GetTypeInfo(parsedValue.Key);
                if (table == null)
                {
                    Debug.LogWarning("Serialization info not created");
                    continue;
                }

                foreach (var pair in parsedValue.Value) table.PrepareDb(this, pair.Value);
            }
            
            //serialize all exist objects
            foreach (var parsedValue in _parsedValues)
            {
                var table = _serializationData.GetTypeInfo(parsedValue.Key);
                if (table == null)
                {
                    Debug.LogWarning("Serialization info not created");
                    continue;
                }

                var e = new List<KeyValuePair<string, string>>();
                foreach (var o in parsedValue.Value.Values)
                {
                    var json = table.Serialize(o);
                    e.Add(new KeyValuePair<string, string>(table.GetId(o).ToString(), json.ToString()));
                }

                _values[parsedValue.Key] = new TableInfo { Id = table.Id, Values = e };
            }

#if UNITY_EDITOR
            return JsonConvert.SerializeObject(_values, Formatting.Indented);
#else
            return JsonConvert.SerializeObject(_values, Formatting.None);
#endif
        }

        public object Get(Type type, object id)
        {
            var list = Get(type);
            return list.TryGetValue(id.ToString(), out var result) ? result : default;
        }

        public void Add(object o)
        {
            var tableType = o.GetType();
            var typeInfo = _serializationData.GetTypeInfo(tableType);
            var parsedList = Get(tableType);
            var key = typeInfo.GetId(o);
            parsedList[key.ToString()] = o;
        }

        public void Remove(object o)
        {
            var tableType = o.GetType();
            var typeInfo = _serializationData.GetTypeInfo(tableType);
            var parsedList = Get(tableType);
            var key = typeInfo.GetId(o);
            parsedList.Remove(key.ToString());
        }
    }
}