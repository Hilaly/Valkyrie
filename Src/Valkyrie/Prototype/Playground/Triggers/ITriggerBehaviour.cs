using System;
using UnityEngine;

namespace Valkyrie.Playground.Triggers
{
    public interface ITriggerBehaviour
    {
        event Action<Collider> TriggerEnter;
        event Action<Collider> TriggerStay;
        event Action<Collider> TriggerExit;
    }
}