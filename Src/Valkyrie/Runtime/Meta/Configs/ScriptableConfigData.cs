using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    public abstract class ScriptableConfigData : ScriptableObject, IConfigData
    {
        public string id;
        public string GetId() => id;

        public virtual void PastLoad(IDictionary<string, IConfigData> configData) { }
    }
}