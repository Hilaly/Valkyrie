using UnityEngine;

namespace Valkyrie.Utils
{
    public static class UtilsExtensions
    {
        #region Camera Controller

        public static Vector3 GetCameraForwardXZ(this ICameraController cameraController)
        {
            var result = cameraController.Camera.transform.forward;
            result.y = 0;
            return result.normalized;
        }

        public static Vector3 GetCameraRightXZ(this ICameraController cameraController)
        {
            var result = cameraController.Camera.transform.right;
            result.y = 0;
            return result.normalized;
        }

        public static Vector3 ConvertToCameraXZ(this ICameraController cameraController, Vector2 joystickValue)
        {
            var tr = cameraController.Camera.transform;
            
            return cameraController.GetCameraForwardXZ() * joystickValue.y + cameraController.GetCameraRightXZ() * joystickValue.x;
        }

        #endregion
    }
}