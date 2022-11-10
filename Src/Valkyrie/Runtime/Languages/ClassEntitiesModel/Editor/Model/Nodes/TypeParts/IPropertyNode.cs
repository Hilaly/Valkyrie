namespace Valkyrie.Model.Nodes
{
    interface IPropertyNode
    {
        PropertyDefine Output { get; }
        
        IFlow FlowOutput { get; }
    }
}