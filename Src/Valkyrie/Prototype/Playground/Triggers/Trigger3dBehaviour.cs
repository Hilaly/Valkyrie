using System;
using UnityEngine;

namespace Valkyrie.Playground.Triggers
{
    public class Trigger3dBehaviour : MonoBehaviour, ITriggerBehaviour
    {
        public event Action<Collider> TriggerEnter;
        public event Action<Collider> TriggerStay;
        public event Action<Collider> TriggerExit;
        
        private void OnTriggerEnter(Collider other) => TriggerEnter?.Invoke(other);
        private void OnTriggerStay(Collider other) => TriggerStay?.Invoke(other);
        private void OnTriggerExit(Collider other) => TriggerExit?.Invoke(other);
    }
}