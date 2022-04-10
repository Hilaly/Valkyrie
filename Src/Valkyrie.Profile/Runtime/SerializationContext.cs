using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Valkyrie.Profile
{
    class SerializationContext
    {
        private readonly SerializationData _serializationData;
        private readonly Dictionary<string, TableInfo> _values;
        private readonly Dictionary<string, Dictionary<string, object>> _parsedValues = new();
        private readonly HashSet<Type> _unsupportedTypes = new HashSet<Type>();

        public SerializationContext(SerializationData serializationData, string json, object dbContext)
        {
            _serializationData = serializationData;
            _values = JsonConvert.DeserializeObject<Dictionary<string, TableInfo>>(json);
            _unsupportedTypes.Add(dbContext.GetType());

            TryDeserializeObject(dbContext);
        }

        private void TryDeserializeObject(object value)
        {
            var tableType = value.GetType();
            var typeInfo = _serializationData.GetTypeInfo(tableType);
            var tableName = typeInfo.Table;
            if (_values.TryGetValue(tableName, out var unparsedList) && unparsedList.Values.Count > 0)
            {
                var pair = unparsedList.Values.FindLast(x => true);
                var jo = JObject.Parse(pair.Value);
                typeInfo.Deserialize(value, jo);
                typeInfo.SetReferences(this, value, jo);
                
                Add(value);
            }
        }

        private Dictionary<string, object> Get(Type tableType)
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

        void Deserialize(TableInfo unparsedList, TypeSerializationInfo typeInfo, Dictionary<string, object> parsedList)
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
            foreach (var parsedValue in _parsedValues)
            {
                var table = _serializationData.GetTypeInfo(parsedValue.Key);
                if (table == null)
                {
                    Debug.LogWarning($"Serialization info not created");
                    continue;
                }

                var e = new List<KeyValuePair<string, string>>();
                foreach (var o in parsedValue.Value.Values)
                {
                    var json = table.Serialize(o);
                    e.Add(new KeyValuePair<string, string>(table.GetId(o).ToString(), json.ToString()));
                }

                _values[parsedValue.Key] = new TableInfo() { Id = table.Id, Values = e };
            }

            return JsonConvert.SerializeObject(_values);
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