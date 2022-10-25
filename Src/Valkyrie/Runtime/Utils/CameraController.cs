using UnityEngine;

namespace Valkyrie.Utils
{
    public interface ICameraController
    {
        Camera Camera { get; }

        float Height { get; set; }
        float Yaw { get; set; }
        float Pitch { get; set; }
        float Distance { get; set; }

        void SetTarget(Vector3 position, Quaternion rotation);
        void MoveTo(Vector3 position, float speed);
    }

    public class CameraController : MonoBehaviour, ICameraController
    {
        class CameraMoveParameters
        {
            public Vector3 TargetPoint;
            public float Speed;
        }

        [SerializeField] private bool followRotation;

        [SerializeField] private Transform heightController;
        [SerializeField] private Transform yawController;
        [SerializeField] private Transform pitchController;
        [SerializeField] private Transform distanceController;
        [SerializeField] private Camera _camera;

        private CameraMoveParameters _moving;

        public Camera Camera => _camera;

        public float Height
        {
            get => heightController.localPosition.y;
            set => heightController.localPosition = new Vector3(0, value, 0);
        }

        public float Yaw
        {
            get => yawController.localRotation.eulerAngles.y;
            set => yawController.localRotation = Quaternion.AngleAxis(value, Vector3.up);
        }

        public float Pitch
        {
            get => pitchController.localRotation.eulerAngles.x;
            set => pitchController.localRotation = Quaternion.AngleAxis(value, Vector3.right);
        }

        public float Distance
        {
            get => -distanceController.localPosition.z;
            set => distanceController.localPosition = new Vector3(0, 0, -value);
        }

        public void SetTarget(Vector3 position, Quaternion rotation)
        {
            _moving = null;
            SetTransformInternal(position, rotation);
        }

        public void MoveTo(Vector3 position, float speed)
        {
            _moving = new CameraMoveParameters() { Speed = speed, TargetPoint = position };
        }

        private void LateUpdate()
        {
            if (_moving != null)
                DoMove();
        }

        private void DoMove()
        {
            var np = Vector3.MoveTowards(transform.position, _moving.TargetPoint, Time.deltaTime * _moving.Speed);
            SetTransformInternal(np, transform.rotation);
            if (np == _moving.TargetPoint)
                _moving = null;
        }

        void SetTransformInternal(Vector3 position, Quaternion rotation)
        {
            if (followRotation)
                transform.SetPositionAndRotation(position, rotation);
            else
                transform.position = position;
        }
    }
}