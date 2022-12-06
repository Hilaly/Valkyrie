using UnityEngine;

namespace Valkyrie.Playground.Features
{
    class RotationAbilityComponent : MonoComponent, IRotationAbilityComponent
    {
        [field:SerializeField] public float RotationSpeed { get; set; }
    }
}