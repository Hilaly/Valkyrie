using System.Collections.Generic;

namespace Valkyrie.DSL.Actions
{
    class LocalVariableStringProvider : IStringProvider
    {
        private readonly string _varName;

        public LocalVariableStringProvider(string varName) => _varName = varName;

        public string GetString(Dictionary<string, string> localVariables, Dictionary<string, string> globalVariables) 
            => localVariables[_varName];

        public override string ToString() => $"${_varName}";
    }
}