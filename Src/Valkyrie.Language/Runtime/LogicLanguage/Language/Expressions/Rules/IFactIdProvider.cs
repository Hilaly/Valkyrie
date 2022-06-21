namespace Valkyrie.Language.Language.Expressions.Rules
{
    interface IFactIdProvider : IRuntimeCheck
    {
        int FactId { get; }
        IFactIdProvider NextProvider { get; set; }
    }
}