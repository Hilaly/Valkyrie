using UnityEngine;

namespace Utils.Pool
{
    public interface IObjectsPool
    {
        IPooledInstance<T> Instantiate<T>(string prefabName) where T : Component;
        IPooledInstance<T> Instantiate<T>(string prefabName, Vector3 position, Quaternion rotation) where T : Component;
    }
}