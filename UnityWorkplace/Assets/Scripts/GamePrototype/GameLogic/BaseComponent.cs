using UnityEngine;

namespace GamePrototype.GameLogic
{
    public abstract class BaseComponent
    {
        
    }

    public abstract class ValueComponent<T> : BaseComponent
    {
        public T Value;
    }
    
    public class PositionComponent : ValueComponent<Vector3>
    {}
}