using System.Collections.Generic;
using Configs;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using Valkyrie.Entities;

namespace Valkyrie.Language
{
    public class ConfigTests
    {
        [Test]
        public void SaveConfigToString()
        {
            var list = new List<IConfigData>
            {
                new Entity("23"),
                new Entity("123")
            };

            var s = JsonConvert.SerializeObject(list, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
            
            Debug.Log(s);
        }

        [Test]
        public void LoadSaveConfigFromString()
        {
            var s = "{ \"$values\":[{\"$type\":\"Valkyrie.Entities.Entity, Valkyrie.Entities.Complex\",  \"Id\":\"23\"}]}";

            var list = JsonConvert.DeserializeObject<List<IConfigData>>(s, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto,
            });
            
            Assert.AreEqual(1, list.Count);
        }
    }
}