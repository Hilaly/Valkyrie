using System.Collections.Generic;

namespace Valkyrie.Profile
{
    internal class TableInfo
    {
        public ulong Id = 1;
        public List<KeyValuePair<string, string>> Values = new();
    }
}