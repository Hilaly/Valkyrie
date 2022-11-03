using System;
using System.Collections.Generic;

namespace Valkyrie
{
    [Serializable]
    public abstract class MainFeatureData : Graph
    {
        public string name;
        public string displayName;
        public string description;

        public Dictionary<string, string> dependencies = new();
    }
}