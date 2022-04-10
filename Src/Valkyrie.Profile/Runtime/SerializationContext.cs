using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Valkyrie.Profile
{
    class TableInfo
    {
        public ulong Id = 1;
        public List<string> Values = new List<string>();
    }
    class SerializationContext
    {
        [NonSerialized] private readonly Dictionary<string, Dictionary<object, object>> ParsedValues = new();
        private readonly Dictionary<string, TableInfo> Values;

        public SerializationContext(string json)
        {
            Values = JsonConvert.DeserializeObject<Dictionary<string, TableInfo>>(json);
        }

        public SerializationContext()
        {
            Values = new Dictionary<string, TableInfo>();
        }

        public Dictionary<object, object> Get(DbTableDesc tableDesc)
        {
            if (!ParsedValues.TryGetValue(tableDesc.Name, out var parsedList))
            {
                ParsedValues.Add(tableDesc.Name, parsedList = new Dictionary<object, object>());
                if (Values.TryGetValue(tableDesc.Name, out var unparsedList))
                {
                    tableDesc.Id = unparsedList.Id;
                    foreach (var unparsedValue in unparsedList.Values)
                    {
                        var value = tableDesc.Deserialize(unparsedValue);
                        var key = tableDesc.GetIt(value);
                        parsedList.Add(key, value);
                    }
                }
            }

            return parsedList;
        }

        public object Get(DbTableDesc tableDesc, object id)
        {
            var parsedList = Get(tableDesc);

            return parsedList.TryGetValue(id, out var parsedObject) ? parsedObject : null;
        }

        public string Serialize(List<DbTableDesc> tables)
        {
            foreach (var parsedValue in ParsedValues)
            {
                var table = tables.Find(x => x.Name == parsedValue.Key);
                var e = new List<string>();
                foreach (var o in parsedValue.Value.Values)
                {
                    var json = table.Serialize(o);
                    e.Add(json);
                }

                Values.Add(parsedValue.Key, new TableInfo() { Id = table.Id, Values = e });
            }
            return JsonConvert.SerializeObject(Values);
        }

        public void Add(DbTableDesc table, object o)
        {
            var parsedList = Get(table);
            var key = table.GetIt(o);
            parsedList.Add(key, o);
        }
    }
}