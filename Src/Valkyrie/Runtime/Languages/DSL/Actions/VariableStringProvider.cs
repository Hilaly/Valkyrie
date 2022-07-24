using System.Collections.Generic;

namespace Valkyrie.DSL.Actions
{
    class VariableStringProvider : IStringProvider
    {
        private readonly string _varName;

        public VariableStringProvider(string varName) => _varName = varName;

        public string GetString(Dictionary<string, string> args) => args[_varName];

        public override string ToString() => $"${_varName}";
    }
}