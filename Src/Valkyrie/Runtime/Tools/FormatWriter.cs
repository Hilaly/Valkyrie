using System.Text;

namespace Valkyrie.Language.Description.Utils
{
    public class FormatWriter
    {
        private readonly StringBuilder _sb = new();

        private string _lineStart = string.Empty;

        public void AddTab() => _lineStart += '\t';
        public void RemoveTab() => _lineStart = _lineStart.Substring(0, _lineStart.Length - 1);

        public override string ToString() => _sb.ToString();

        public FormatWriter BeginBlock(string namespaceName = null)
        {
            if(!string.IsNullOrEmpty(namespaceName))
                AppendLine($"{namespaceName}");
            AppendLine("{");
            AddTab();
            return this;
        }

        public FormatWriter BeginForBlock(string indexName, string minValue, string maxValue) => 
            BeginBlock($"for (var {indexName} = {minValue}; {indexName} < {maxValue}; ++{indexName})");

        public FormatWriter BeginForIterationBlock(string varName, string collectionName, string indexerName = "index") =>
            BeginForBlock(indexerName, "0", $"{collectionName}.Count")
                .AppendLine($"var {varName} = {collectionName}[{indexerName}];");

        public FormatWriter BeginForEachIterationBlock(string varName, string collectionName) =>
            BeginBlock($"foreach (var {varName} in {collectionName})");

        public FormatWriter EndBlock()
        {
            RemoveTab();
            AppendLine("}");
            return this;
        }

        public FormatWriter AppendLine()
        {
            _sb.Append(_lineStart).AppendLine();
            return this;
        }

        public FormatWriter AppendLine(string text)
        {
            _sb.Append(_lineStart).AppendLine(text);
            return this;
        }
    }
}