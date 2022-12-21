using System.Collections.Generic;

namespace Valkyrie.DSL.StringWorking
{
    class GlobalVariableStringProvider : IStringProvider
    {
        private readonly string _varName;

        public GlobalVariableStringProvider(string varName) => _varName = varName;

        public string GetString(Dictionary<string, string> localVariables, Dictionary<string, string> globalVariables) 
            => globalVariables[_varName];
        
        public override string ToString() => $"${_varName}";
    }
}