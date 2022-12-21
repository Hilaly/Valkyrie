using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using Utils;

namespace Valkyrie.Utils
{
    public static class ClassModelSerializer
    {
        public const string DefaultLocation = "ProjectSettings/Valkyrie";
        public const string DefaultFileName = "WorldModel.json";

        private static WorldModelInfo _currentWorld;

        private static readonly JsonSerializerSettings SerializeSettings = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static WorldModelInfo Load(string dirPath = DefaultLocation, string fileName = DefaultFileName)
        {
            var fullFileName = EnsureDirectory(dirPath, fileName);

            if (!File.Exists(fullFileName))
            {
                Debug.LogWarning($"File {fullFileName} couldn't be found");
                return new WorldModelInfo();
            }

            var json = File.ReadAllText(fullFileName);
            var jo = JObject.Parse(json);
            var world = _currentWorld = jo.ToObject<WorldModelInfo>();
            ReadCustom(jo, world);
            _currentWorld = default;
            return world;
        }

        public static void Save(this WorldModelInfo world, string dirPath = DefaultLocation,
            string fileName = DefaultFileName)
        {
            var fullFileName = EnsureDirectory(dirPath, fileName);

            var jo = JObject.FromObject(world);

            WriteCustom(jo, world);

            File.WriteAllText(fullFileName, jo.ToString());
            Debug.Log($"World saved to {fullFileName}");
        }

        private static string EnsureDirectory(string dirPath, string fileName)
        {
            var fullFileName = Path.Combine(dirPath, fileName);
            var fullDirName = Path.GetDirectoryName(fullFileName);
            if (fullDirName != null && !Directory.Exists(fullDirName))
            {
                Debug.Log($"Creating directory {fullDirName}");
                Directory.CreateDirectory(fullDirName);
            }

            return fullFileName;
        }

        private static void WriteCustom(JObject jo, Feature feature)
        {
            var types = feature.Get<BaseType>();
            if (types.Count == 0)
                return;

            var list = new JArray();

            foreach (var baseType in types)
                list.Add(WriteCustom(baseType));

            jo.Add("types", list);
        }

        private static void ReadCustom(JObject jo, Feature feature)
        {
            var types = typeof(BaseType).GetAllSubTypes(x => x.IsClass && !x.IsAbstract);
            var aToken = jo["types"];
            if (aToken != null)
            {
                var read = new Dictionary<JObject, BaseType>();
                foreach (var jToken in aToken)
                {
                    var typeObject = (JObject)jToken;
                    var fullTypeName = typeObject.Value<string>("$type");
                    var typeName = typeObject.Value<string>("name");
                    var instType = types.Find(x => x.Name == fullTypeName || x.FullName == fullTypeName);
                    var inst = (BaseType)Activator.CreateInstance(instType);
                    inst.Name = typeName;
                    read.Add(typeObject, inst);
                    feature.Push(inst);
                }

                foreach (var pair in read)
                {
                    ReadCustom(pair.Key, pair.Value);
                }
            }
        }

        static void ReadArray(this JObject jo, string propertyName, Action<JToken> call)
        {
            var p = jo[propertyName];
            if (p == null)
                return;
            foreach (var value in p)
                call(value);
        }

        static BaseType FindType(string id) => _currentWorld.Get<BaseType>().First(x => x.Name == id);

        private static void ReadCustom(JObject jo, BaseType baseType)
        {
            jo.ReadArray("attributes", value => baseType.AddAttribute((string)value));
            jo.ReadArray("parents", value => baseType.Inherit(FindType((string)value)));
            jo.ReadArray("timers", value => baseType.AddTimer((string)value));
            jo.ReadArray("prefabs", value =>
            {
                var t = value.ToObject<ViewSpawnInfo>();
                baseType.ViewWithPrefabByProperty(t.PropertyName, t.ViewName);
            });
            jo.ReadArray("properties", value => ReadCustomProperty((JObject)value, baseType));
            jo.ReadArray("infos", value => ReadCustomInfo((JObject)value, baseType));
        }

        private static void ReadCustomInfo(JObject jo, BaseType baseType)
        {
            baseType.Infos.Add(new BaseTypeInfo()
            {
                Name = jo.Value<string>("name"),
                Code = jo.Value<string>("code"),
                TypeData = jo.ReadCustomType("typeData")
            });
        }

        private static void ReadCustomProperty(JObject jo, BaseType baseType)
        {
            baseType.Properties.Add(new BaseTypeProperty()
            {
                Name = jo.Value<string>("name"),
                IsRequired = jo.Value<bool>("required"),
                TypeData = jo.ReadCustomType("typeData")
            });
        }

        private static JToken WriteCustom(BaseType baseType)
        {
            var jo = new JObject
            {
                { "$type", baseType.GetType().FullName },
                { "name", baseType.Name },
            };

            if (baseType.Attributes.Any())
                jo.Add("attributes", JArray.FromObject(baseType.Attributes));
            if (baseType.BaseTypes.Any())
                jo.Add("parents", JArray.FromObject(baseType.BaseTypes.Select(x => x.Name)));
            if (baseType.Timers.Any())
                jo.Add("timers", JArray.FromObject(baseType.Timers));
            if (baseType.GetPrefabsProperties().Any())
                jo.Add("prefabs", JArray.FromObject(baseType.GetPrefabsProperties()));
            if (baseType.Properties.Any())
                jo.Add("properties", WriteCustom(baseType.Properties));
            if (baseType.Infos.Any())
                jo.Add("infos", WriteCustom(baseType.Infos));

            return jo;
        }

        private static JToken WriteCustom(List<BaseTypeInfo> members)
        {
            var list = new JArray();

            foreach (var member in members)
                list.Add(new JObject
                    {
                        { "name", member.Name },
                        { "code", member.Code }
                    }
                    .WriteCustom("typeData", member.TypeData));

            return list;
        }

        private static JToken WriteCustom(List<BaseTypeProperty> members)
        {
            var list = new JArray();

            foreach (var member in members)
                list.Add(new JObject
                    {
                        { "name", member.Name },
                        { "required", member.IsRequired }
                    }
                    .WriteCustom("typeData", member.TypeData));

            return list;
        }

        private static TypeData ReadCustomType(this JObject jo, string fieldName)
        {
            var temp = jo[fieldName];
            var value = temp.Value<string>("value");
            var type = temp.Value<string>("type");
            switch (type)
            {
                case "Valkyrie.CSharpTypeData":
                case "CSharpTypeData":
                    return new CSharpTypeData(value.FindType());
                case "Valkyrie.RefTypeData":
                case "RefTypeData":
                    return new RefTypeData(FindType(value));
                default:
                    throw new Exception($"Failed to deserialize TypeData {jo}");
            }
        }

        private static JObject WriteCustom(this JObject jo, string fieldName, TypeData td)
        {
            jo.Add(fieldName, new JObject()
            {
                { "type", td.GetType().FullName },
                { "value", td.GetTypeName() }
            });
            return jo;
        }
    }
}