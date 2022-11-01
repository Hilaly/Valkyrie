using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie
{
    public class EventEntity : INamed
    {
        public string Name { get; set; }
        public readonly List<string> Args = new();

        public string ClassName => $"{Name}Event";

        public void Write(FormatWriter sb)
        {
            var blockName = $"public sealed class {ClassName} : {typeof(BaseEvent).FullName}";
            if (Args.Any())
                blockName += $"<{Args.Join(", ")}>";
            sb.BeginBlock(blockName);
            sb.EndBlock();
        }
    }
}