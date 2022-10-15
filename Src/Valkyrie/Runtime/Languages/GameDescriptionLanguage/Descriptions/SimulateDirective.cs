namespace Valkyrie.Language.Description
{
    public class SimulateDirective : ISimPart
    {
        public string Name;

        public string GetListName() => $"_{Name}List";
        public string GetTypeName() => $"I{Name}";
    }
}