using System.Collections.Generic;
using UnityEngine;

namespace Configs
{
    public abstract class ScriptableConfigData : ScriptableObject
    {
        public string GetId() => name;

        public virtual void PastLoad(IDictionary<string, IConfigData> configData) { }
    }
}