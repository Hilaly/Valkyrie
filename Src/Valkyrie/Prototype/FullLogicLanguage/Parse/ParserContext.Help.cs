using UnityEngine;
using Valkyrie.Playground;

namespace Valkyrie
{
    partial class ParserContext
    {
        public string ApplyAliases(string value) =>
            Aliases.TryGetValue(value, out var text) ? text : value;

        public ParserContext FillDefault()
        {
            void ImportNative<T>() => Game.Import(typeof(T));
            
            void PrepareNative<T>(string alias)
            {
                AddAlias(alias, typeof(T).FullName);
                ImportNative<T>();
            }
            
            PrepareNative<float>("float");
            PrepareNative<int>("int");
            PrepareNative<bool>("bool");
            PrepareNative<string>("string");

            PrepareNative<Vector2>("vec2");
            PrepareNative<Vector3>("vec3");

            ImportNative<IArchetype>();

            return this;
        }
    }
}