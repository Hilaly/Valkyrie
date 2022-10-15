using System.Collections.Generic;

namespace Valkyrie.Language.Ecs
{
    class StringToIntConverter
    {
        private readonly List<string> _names = new List<string>();

        public int Count => _names.Count;

        public int GetId(string value)
        {
            var index = _names.IndexOf(value);
            if (index < 0)
            {
                index = _names.Count;
                _names.Add(value);
            }

            return index;
        }

        public string GetString(int id) => _names[id];
        public bool HasString(string name) => _names.Contains(name);
    }
}