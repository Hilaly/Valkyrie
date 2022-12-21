namespace Valkyrie.DSL.Dictionary
{
    public interface IDslMacro
    {
        string Pattern { get; }
        string Replacement { get; }
        bool IsMatch(string text);
        string Apply(string text);
    }
}