using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.DSL
{
    public class CompilerContext
    {
        public string Namespace;
        public readonly List<string> Usings = new();

        public readonly List<GeneratedTypeDefinition> Types = new();

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

    public interface IWritable
    {
        void Write(FormatWriter sb);
    }

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