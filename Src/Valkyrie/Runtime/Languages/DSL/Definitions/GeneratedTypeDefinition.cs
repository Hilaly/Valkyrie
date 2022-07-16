using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Definitions
{
    public class GeneratedTypeDefinition : IWritable
    {
        public string TypeCategory { get; set; }
        public string Name { get; set; }
        public List<string> BaseTypes { get; } = new();

        public void Write(FormatWriter sb)
        {
            var classDef = Name;
            if (BaseTypes.Any()) 
                classDef += " : " + BaseTypes.Join(", ");
            sb.BeginBlock($"public class {classDef}");
            sb.EndBlock();
        }
    }
}