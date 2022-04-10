using System;
using Newtonsoft.Json;

namespace Valkyrie.Profile
{
    class DbTableDesc
    {
        private readonly Func<DbTableDesc, object, object> _idFunc;
        private readonly Func<string, object> _deserFunc;
        private readonly Func<object, string> _serFunc;

        public DbTableDesc()
        {
        }

        public DbTableDesc(string name, Type tableType,
            Func<DbTableDesc, object, object> idFunc,
            Func<string, object> deserFunc, Func<object, string> serFunc)
        {
            _idFunc = idFunc;
            _deserFunc = deserFunc;
            _serFunc = serFunc;
            Name = name;
            Type = tableType;
        }

        public string Name { get; set; }

        [JsonIgnore] public Type Type { get; set; }
        public ulong Id { get; set; } = 1;

        public object GetIt(object o) => _idFunc(this, o);
        public object Deserialize(string jsonStr) => _deserFunc(jsonStr);

        public string Serialize(object o) => _serFunc(o);
    }
}