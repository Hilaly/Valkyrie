using UnityEngine;
using Valkyrie.Playground;

namespace Project.Playground.Features.MonoImplementations
{
    class TransformComponent : MonoComponent, ITransformComponent
    {
        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Vector3 Direction
        {
            get => transform.forward;
            set => transform.forward = value;
        }

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }
    }
}