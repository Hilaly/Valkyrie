using System.Collections.Generic;

namespace Valkyrie.DSL.StringWorking
{
    class ConstantStringProvider : IStringProvider
    {
        private readonly string _value;

        public ConstantStringProvider(string value) => _value = value.Trim('"');

        public string GetString(Dictionary<string, string> localVariables, Dictionary<string, string> globalVariables) => _value;

        public override string ToString() => _value;
    }
}