using UnityEngine;

namespace GamePrototype.Mono
{
    public class SpawnChildBehaviour : MonoBehaviour
    {
        private GameObject _spawned;
        private string _lastSpawned;
        
        public string PrefabName
        {
            get => _lastSpawned;
            set
            {
                if(_lastSpawned == value)
                    return;
                _lastSpawned = value;
                if (_spawned != null)
                {
                    Destroy(_spawned);
                    _spawned = null;
                }

                _spawned = Instantiate(Resources.Load<GameObject>(_lastSpawned), transform);
            }
        }
    }
}