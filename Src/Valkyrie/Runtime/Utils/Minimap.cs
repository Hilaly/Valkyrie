using UnityEngine;

namespace Utils
{
    public class Minimap : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public Camera Camera => _camera;

        public float Size
        {
            get => _camera.orthographicSize;
            set => _camera.orthographicSize = value;
        }
    }
}