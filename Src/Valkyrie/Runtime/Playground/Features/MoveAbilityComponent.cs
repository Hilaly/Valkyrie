using UnityEngine;

namespace Valkyrie.Playground.Features
{
    class MoveAbilityComponent : MonoComponent, IMoveAbilityComponent
    {
        [field:SerializeField] public float MoveSpeed { get; set; }
    }
}