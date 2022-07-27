using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Actions
{
    class ConcatenateStringProvider : IStringProvider, IEnumerable<IStringProvider>
    {
        private readonly List<IStringProvider> _providers = new();

        public void Add(IStringProvider provider) => _providers.Add(provider);

        public string GetString(Dictionary<string, string> localVariables, Dictionary<string, string> globalVariables) =>
            _providers
                .Select(x => x.GetString(localVariables, globalVariables))
                .Join(string.Empty);

        public IEnumerator<IStringProvider> GetEnumerator() => _providers.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => _providers.Select(x => x.ToString()).Join("+");
    }
}