using System.Collections.Generic;

namespace Valkyrie.DSL.Actions
{
    class ConstantStringProvider : IStringProvider
    {
        private readonly string _value;

        public ConstantStringProvider(string value) => _value = value.Trim('"');

        public string GetString(Dictionary<string, string> args) => _value;

        public override string ToString() => _value;
    }
}