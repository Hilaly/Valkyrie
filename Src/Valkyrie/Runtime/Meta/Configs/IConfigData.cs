using System.Collections.Generic;

namespace Valkyrie.Meta.Configs
{
    public interface IConfigData
    {
        public string GetId();
        public void PastLoad(IDictionary<string, IConfigData> configData);
    }
}