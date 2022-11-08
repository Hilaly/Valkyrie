namespace Valkyrie.Model
{
    public interface INodeFactory
    {
        string Name { get; }
        string Path { get; }
        
        INode Create(IGraph graph);
    }
}