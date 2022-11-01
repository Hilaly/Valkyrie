using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Valkyrie.Utils
{
    public static class ClassModelSerializer
    {
        public const string DefaultLocation = "ProjectSettings/Valkyrie";
        public const string DefaultFileName = "WorldModel.json";

        private static readonly JsonSerializerSettings SerializeSettings = new JsonSerializerSettings()
        {
            Culture = CultureInfo.InvariantCulture,
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto
        };

        public static WorldModelInfo Load(string dirPath = DefaultLocation, string fileName = DefaultFileName)
        {
            var fullFileName = Path.Combine(dirPath, fileName);
            var fullDirName = Path.GetDirectoryName(fullFileName);
            if (!Directory.Exists(fullDirName))
            {
                Debug.Log($"Creating directory {fullDirName}");
                Directory.CreateDirectory(fullDirName);
            }

            if (!File.Exists(fullFileName))
            {
                Debug.LogWarning($"File {fullFileName} couldn't be found");
                return new WorldModelInfo();
            }

            var json = File.ReadAllText(fullFileName);
            var jo = JObject.Parse(json);
            var world = jo.ToObject<WorldModelInfo>();
            return new WorldModelInfo();
        }

        public static void Save(this WorldModelInfo world, string dirPath = DefaultLocation,
            string fileName = DefaultFileName)
        {
            var fullFileName = Path.Combine(dirPath, fileName);
            var fullDirName = Path.GetDirectoryName(fullFileName);
            if (!Directory.Exists(fullDirName))
            {
                Debug.Log($"Creating directory {fullDirName}");
                Directory.CreateDirectory(fullDirName);
            }

            var jo = JObject.FromObject(world);

            WriteCustom(jo, world);

            File.WriteAllText(fullFileName, jo.ToString());
            Debug.Log($"World saved to {fullFileName}");
        }

        private static void WriteCustom(JObject jo, WorldModelInfo world)
        {
            var list = new JArray();

            foreach (var baseType in world.Get<BaseType>())
                list.Add(WriteCustom(baseType));

            jo.Add("types", list);
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
                        { "name", member.Name }
                    }
                    .WriteCustom("type", member.TypeData));

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
                    .WriteCustom("type", member.TypeData));

            return list;
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