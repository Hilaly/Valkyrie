namespace Valkyrie
{
    public abstract class GenericNode<T> : BaseNode where T : INode, new()
    {
        private static readonly Factory FactoryInstance = new();
        
        class Factory : GenericNodeFactory<T>
        {
            
        }

        public override INodeFactory GetData() => FactoryInstance;
    }
}