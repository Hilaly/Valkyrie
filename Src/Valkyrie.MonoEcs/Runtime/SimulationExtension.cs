using UnityEngine;

namespace Valkyrie.Ecs
{
    public static class SimulationExtension
    {
        #region Extensions for mono behaviour

        public static Vector3 GetPosition(this Component mb) => mb.transform.position;
        public static Vector3 GetPosition(this GameObject mb) => mb.transform.position;

        public static Vector3 GetForward(this Component mb) => mb.transform.forward;
        public static Vector3 GetForward(this GameObject mb) => mb.transform.forward;

        #endregion
    }
}