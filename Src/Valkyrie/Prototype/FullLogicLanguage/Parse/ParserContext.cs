using System.Collections.Generic;

namespace Valkyrie
{
    partial class ParserContext
    {
        public GameDescription Game;
        public IArchetypeDescription CurrentArchetype { get; set; }

        public readonly Dictionary<string, string> Aliases = new();

        public void TryAddComponent(string componentName, string componentType)
        {
            var realTypeName = ApplyAliases(componentType);
            Game.TryAddComponent(componentName, realTypeName);
        }

        public void AddAlias(string name, string realValue) => Aliases[name] = realValue;
    }
}