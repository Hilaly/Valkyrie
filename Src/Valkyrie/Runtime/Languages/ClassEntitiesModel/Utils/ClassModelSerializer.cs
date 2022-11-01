using System.Globalization;
using System.IO;
using Newtonsoft.Json;
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
            var world = JsonConvert.DeserializeObject<WorldModelInfo>(json);
            return world;
        }

        public static void Save(this WorldModelInfo world, string dirPath = DefaultLocation, string fileName = DefaultFileName)
        {
            var fullFileName = Path.Combine(dirPath, fileName);
            var fullDirName = Path.GetDirectoryName(fullFileName);
            if (!Directory.Exists(fullDirName))
            {
                Debug.Log($"Creating directory {fullDirName}");
                Directory.CreateDirectory(fullDirName);
            }
            var json = JsonConvert.SerializeObject(world, SerializeSettings);
            File.WriteAllText(fullFileName, json);
            Debug.Log($"World saved to {fullFileName}");
        }
    }
}