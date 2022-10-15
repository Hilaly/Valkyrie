using Valkyrie.MVVM;

namespace NaiveEntity.GamePrototype.EntProto.ViewProto
{
    public class EntityView
    {
        [Binding] public IEntity Entity { get; set; }
    }
}