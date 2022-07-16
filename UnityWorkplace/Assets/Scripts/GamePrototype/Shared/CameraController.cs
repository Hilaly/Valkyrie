using UnityEngine;

namespace Hilaly.Utils
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private bool followRotation;
        
        [SerializeField] private Transform heightController;
        [SerializeField] private Transform yawController;
        [SerializeField] private Transform pitchController;
        [SerializeField] private Transform distanceController;
        [SerializeField] private Camera _camera;

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
            if(followRotation)
                transform.SetPositionAndRotation(position, rotation);
            else
                transform.position = position;
        }
    }
}
