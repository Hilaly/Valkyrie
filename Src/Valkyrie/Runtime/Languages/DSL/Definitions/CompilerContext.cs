using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Definitions
{
    public class CompilerContext
    {
        public string Namespace;
        public readonly List<string> Usings = new();

        public readonly List<GeneratedTypeDefinition> Types = new();
        public readonly List<string> UnparsedSentences = new ();
        internal DslCompiler Compiler;
        public Dictionary<string, string> GlobalVariables { get; } = new Dictionary<string, string>();

        public GeneratedTypeDefinition GetOrCreateType(string name)
        {
            var type = Types.Find(x => x.Name == name);
            if (type == null) Types.Add(type = new GeneratedTypeDefinition() { Name = name });

            return type;
        }

        public override string ToString()
        {
            var sb = new FormatWriter();

            //TODO: add file comment
            
            //1. Write usings at start of file
            foreach (var @using in Usings) 
                sb.AppendLine($"using {@using};");
            if (Usings.Any())
                sb.AppendLine();
            
            //2. Start namespace
            if (Namespace.NotNullOrEmpty())
                sb.BeginBlock($"namespace {Namespace}");

            //3. Write all generated types
            foreach (var typeDefinition in Types) 
                typeDefinition.Write(sb);

            //4. Close namespace
            if (Namespace.NotNullOrEmpty())
                sb.EndBlock();

            return sb.ToString();
        }
    }
}